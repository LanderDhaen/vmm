using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;

using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Services;
using Rise.Shared.Common;
using Rise.Shared.Projects;

namespace Rise.Client.Projects {
	public partial class Index {
		private static readonly (string Value, string Label)[] SortColumns = new[]
		{
			("Code","Code"),("Name","Naam"),("StartDate","Startdatum"),("EndDate","Einddatum"),
			("MeasurementNetwork","Meetnet"),("Zone","Zone"),("Status","Status")
		};

		private string _activeSorting = "Name";
		private bool _activeSortingDesc;
		private IEnumerable<ProjectDto.Index>? _mobileItems;
		private bool _mobileSortOptionsOpen;

		private string? _searchString { get; set; } = string.Empty;
		private string _appliedSearch { get; set; } = string.Empty;

		private MudDataGrid<ProjectDto.Index>? _grid;
		private CancellationTokenSource? _typingCts;

		[Inject] public required IProjectService ProjectService { get; init; }
		[Inject] public required NavigationManager NavigationManager { get; init; }
		[Inject] public required BreadcrumbService BreadcrumbService { get; init; }
		[Inject] public required HeaderService HeaderService { get; init; }
		[Inject] public required IBrowserViewportService ViewportService { get; init; }

		protected override async Task OnInitializedAsync() {
			BreadcrumbService.SetBreadcrumbs(new BreadcrumbItem("Home", "/"), new BreadcrumbItem("Projecten", null, true));
			HeaderService.Set("Projecten");

			if (await ViewportService.GetCurrentBreakpointAsync() == Breakpoint.Xs)
				await GetProjectsMobileAsync(_activeSorting, _activeSortingDesc);
		}

		private async Task OnSearchInputChanged() {
			_typingCts?.Cancel();
			_typingCts?.Dispose();
			_typingCts = new CancellationTokenSource();

			try { await Task.Delay(350, _typingCts.Token); } catch (TaskCanceledException) { return; }

			_appliedSearch = string.IsNullOrWhiteSpace(_searchString) ? string.Empty : _searchString!.Trim();
			await ReloadAllAsync(resetPage: true);
		}

		private async Task OnAdornmentClick() {
			if (!string.IsNullOrWhiteSpace(_searchString))
				await ClearSearch();
		}

		private async Task ClearSearch() {
			_typingCts?.Cancel();
			_typingCts?.Dispose();
			_typingCts = null;
			_searchString = string.Empty;
			_appliedSearch = string.Empty;
			await ReloadAllAsync(resetPage: false);
		}

		private async Task ReloadAllAsync(bool resetPage) {
			if (resetPage && _grid is not null) {
				// MudDataGrid has SetCurrentPageAsync in recent versions; fallback to reflection if unavailable
				var setPage = _grid.GetType().GetMethod("SetCurrentPageAsync", new[] { typeof(int) });
				if (setPage != null) await (Task)setPage.Invoke(_grid, new object[] { 0 })!;
			}

			if (_grid is not null)
				await _grid.ReloadServerData();

			await GetProjectsMobileAsync(_activeSorting, _activeSortingDesc);
			StateHasChanged();
		}

		private async Task<GridData<ProjectDto.Index>> GetProjectsAsync(GridState<ProjectDto.Index> state) {
			var sorting = state.SortDefinitions.FirstOrDefault();
			var request = new QueryRequest.SkipTake {
				Skip = state.Page * state.PageSize,
				Take = state.PageSize,
				OrderBy = sorting?.SortBy ?? _activeSorting ?? "Name",
				OrderDescending = sorting?.Descending ?? _activeSortingDesc,
				SearchTerm = string.IsNullOrWhiteSpace(_appliedSearch) ? string.Empty : _appliedSearch.Trim()
			};

			if (state.FilterDefinitions?.Any() == true) {
				foreach (var f in state.FilterDefinitions) {
					var (key, value, op) = ExtractFilter(f);

					// Detect whether operator is an "empty" kind (textual or numeric)
					bool opLooksLikeEmpty = false;
					if (!string.IsNullOrWhiteSpace(op)) {
						var opTrim = op.Trim();
						// textual recognition (e.g. "is not empty", "is empty", "notempty", ...)
						if (Regex.IsMatch(opTrim, "(empty|is\\W*empty|is\\W*not\\W*empty|not\\W*empty)", RegexOptions.IgnoreCase))
							opLooksLikeEmpty = true;
						else if (int.TryParse(opTrim, out var opInt) && (opInt == 6 || opInt == 7))
							// many MudBlazor versions use 6 = IsEmpty, 7 = IsNotEmpty
							opLooksLikeEmpty = true;
					}

					// Send filter if there is a value OR if it's an empty-operator
					if (!string.IsNullOrWhiteSpace(key) && (value != null || opLooksLikeEmpty)) {
						request.Filters[key] = new { Operator = op ?? "Contains", Value = value };
					}
				}
			}

			var result = await ProjectService.GetIndexAsync(request);
			return new GridData<ProjectDto.Index> { TotalItems = result.Value.TotalCount, Items = result.Value.Projects };
		}

		// compact filter extractor: tries recognizable properties and Column.{Field,PropertyName}
		private static (string? Key, object? Value, string? Operator) ExtractFilter(object filterDef) {
			if (filterDef == null) return (null, null, null);

			static object? TryGet(object? obj, params string[] names) {
				if (obj == null) return null;
				var t = obj.GetType();
				foreach (var n in names) {
					var p = t.GetProperty(n, BindingFlags.Public | BindingFlags.Instance);
					if (p != null) return p.GetValue(obj);
				}
				return null;
			}

			// search value
			var rawVal = TryGet(filterDef, "Value", "FilterValue", "Filter", "Values") ?? TryGet(TryGet(filterDef, "Column"), "FilterValue", "Value");
			var rawKey = TryGet(filterDef, "PropertyName", "Field", "MemberName", "ColumnName", "Name")
						 ?? TryGet(TryGet(filterDef, "Column"), "Field", "Property", "PropertyName", "Name");
			var rawOp = TryGet(filterDef, "Operator", "FilterOperator", "OperatorKey", "OperatorName")
						 ?? TryGet(TryGet(filterDef, "Column"), "FilterOperator", "Operator");

			object? valToSend = null;
			if (rawVal is JsonElement je) {
				valToSend = je.ValueKind switch {
					JsonValueKind.Object or JsonValueKind.Array => je.GetRawText(),
					JsonValueKind.String => je.GetString(),
					JsonValueKind.Number => je.GetRawText(),
					JsonValueKind.True => true,
					JsonValueKind.False => false,
					_ => je.GetRawText()
				};
			} else {
				valToSend = rawVal;
			}

			string? key = rawKey?.ToString();
			// normalize p => p.Code or a.b -> Code
			if (!string.IsNullOrWhiteSpace(key)) {
				var s = key!.Trim().Trim('"').Trim('\'');
				if (s.Contains("=>")) s = s.Split("=>").Last().Trim();
				if (s.Contains('.')) s = s.Split('.').Last().Trim();
				key = s;
			}

			return (key, valToSend, rawOp?.ToString());
		}

		private async Task GetProjectsMobileAsync(string orderBy, bool orderDescending) {
			var request = new QueryRequest.SkipTake {
				Skip = 0,
				Take = 10,
				OrderBy = orderBy,
				OrderDescending = orderDescending,
				SearchTerm = string.IsNullOrWhiteSpace(_appliedSearch) ? string.Empty : _appliedSearch.Trim()
			};

			var result = await ProjectService.GetIndexAsync(request);
			_mobileItems = result.Value.Projects;
		}

		private void ToggleSortingDrawer() => _mobileSortOptionsOpen = !_mobileSortOptionsOpen;

		private async Task SortMobileAsync(string orderBy, bool descending) {
			_activeSorting = orderBy;
			_activeSortingDesc = descending;
			await GetProjectsMobileAsync(orderBy, descending);
			ToggleSortingDrawer();
		}

		private void NavigateToProject(Guid projectId) => NavigationManager.NavigateTo($"/projects/{projectId}");
		private void NavigateToCreate() => NavigationManager.NavigateTo("/projects/create");
		private Color DefineColor(Status status) => status == Status.Actief ? Color.Success : Color.Error;
	}
}