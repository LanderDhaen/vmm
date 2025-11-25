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
		private static readonly (string Value, string Label)[] SortColumns = [
			("Code", "Code"),
		("Name", "Naam"),
		("StartDate", "Startdatum"),
		("EndDate", "Einddatum"),
		("MeasurementNetwork", "Meetnet"),
		("Zone", "Zone"),
		("Status", "Status")
		];

		private string _activeSorting = "Name";
		private bool _activeSortingDesc;
		private string _searchTerm;

		private IEnumerable<ProjectDto.Index>? _mobileItems;

		private bool _mobileSortOptionsOpen;

		private CancellationTokenSource _cancellationTokenSource = new();

		private MudDataGrid<ProjectDto.Index> _grid;

		[Inject] public required IProjectService ProjectService { get; init; }
		[Inject] public required NavigationManager NavigationManager { get; init; }
		[Inject] public required BreadcrumbService BreadcrumbService { get; init; }
		[Inject] public required HeaderService HeaderService { get; init; }
		[Inject] public required IBrowserViewportService ViewportService { get; init; }


		protected override async Task OnInitializedAsync() {
			BreadcrumbService.SetBreadcrumbs(
				new BreadcrumbItem("Home", "/"),
				new BreadcrumbItem("Projecten", null, true)
			);

			HeaderService.Set("Projecten");

			Breakpoint breakpoint = await ViewportService.GetCurrentBreakpointAsync();

			if (breakpoint == Breakpoint.Xs) {
				await GetProjectsMobileAsync();
			}
		}

		// Desktop

		private async Task<GridData<ProjectDto.Index>> GetProjectsAsync(GridState<ProjectDto.Index> state) {

			SortDefinition<ProjectDto.Index>? sorting = state.SortDefinitions.FirstOrDefault(
				new SortDefinition<ProjectDto.Index>("Code", false, 0, null));

			QueryRequest.SkipTake request = new() {
				Skip = state.Page * state.PageSize,
				Take = state.PageSize,
				OrderBy = sorting?.SortBy ?? "Name",
				OrderDescending = sorting?.Descending ?? false,
				SearchTerm = string.IsNullOrWhiteSpace(_searchTerm) ? string.Empty : _searchTerm.Trim()
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

		private async Task GetProjectsMobileAsync() {

			_cancellationTokenSource.Cancel();
			_cancellationTokenSource.Dispose();
			_cancellationTokenSource = new CancellationTokenSource();

			QueryRequest.SkipTake request = new() {
				Skip = 0,
				Take = 10,
				OrderBy = _activeSorting,
				OrderDescending = _activeSortingDesc,
				SearchTerm = string.IsNullOrWhiteSpace(_searchTerm) ? string.Empty : _searchTerm.Trim()
			};

			var result = await ProjectService.GetIndexAsync(request);
			_mobileItems = result.Value.Projects;
		}

		private void ToggleSortingDrawer() => _mobileSortOptionsOpen = !_mobileSortOptionsOpen;

		private async Task SortMobileAsync(string orderBy, bool descending) {
			_activeSorting = orderBy;
			_activeSortingDesc = descending;
			await GetProjectsMobileAsync();
			ToggleSortingDrawer();
		}
		// Searching

		private async Task HandleDesktopIntervalElapsed() {
			await _grid.ReloadServerData();
		}

		private async Task HandleMobileIntervalElapsed() {
			await GetProjectsMobileAsync();
		}
		private void NavigateToProject(Guid projectId) => NavigationManager.NavigateTo($"/projects/{projectId}");
		private void NavigateToCreate() => NavigationManager.NavigateTo("/projects/create");
		private Color DefineColor(Status status) => status == Status.Actief ? Color.Success : Color.Error;
	}
}