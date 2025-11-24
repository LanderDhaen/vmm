using Microsoft.AspNetCore.Components;


using MudBlazor;

using Rise.Client.Services;
using Rise.Shared.Common;
using Rise.Shared.Projects;
using System.Text.Json;
using System.Reflection;
using System.Collections;

namespace Rise.Client.Projects {
	public partial class Index {
		private static readonly (string Value, string Label)[] SortColumns = new[]
		{
			("Code", "Code"),
			("Name", "Naam"),
			("StartDate", "Startdatum"),
			("EndDate", "Einddatum"),
			("MeasurementNetwork", "Meetnet"),
			("Zone", "Zone"),
			("Status", "Status")
		};

		private string _activeSorting = "Name";
		private bool _activeSortingDesc;
		private IEnumerable<ProjectDto.Index>? _mobileItems;
		private bool _mobileSortOptionsOpen;

		// Two-tiered search state
		private string? _searchString { get; set; } = string.Empty; // what the user types (bound)
		private string _appliedSearch { get; set; } = string.Empty;  // what is sent to the server

		// grid reference so we can reload it
		private MudDataGrid<ProjectDto.Index>? _grid;

		// Debounce support
		private CancellationTokenSource? _typingCts;

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

			if (await ViewportService.GetCurrentBreakpointAsync() == Breakpoint.Xs)
				await GetProjectsMobileAsync(_activeSorting, _activeSortingDesc);
		}

		// Debounced input change
		private async Task OnSearchInputChanged() {
			// cancel previous pending
			_typingCts?.Cancel();
			_typingCts?.Dispose();

			_typingCts = new CancellationTokenSource();
			try {
				await Task.Delay(350, _typingCts.Token);
			} catch (TaskCanceledException) {
				return;
			}

			await ApplySearchAsync();
		}


		// Apply filter and reload server-side grid + mobile list
		private async Task ApplySearchAsync() {
			_appliedSearch = string.IsNullOrWhiteSpace(_searchString) ? string.Empty : _searchString.Trim();
			Console.WriteLine($"[Client] Applying search: '{_appliedSearch}'");

			await Task.Yield();
			await ReloadAllAsync(resetPage: true);
		}
		// Adornment click handler used by the Razor markup.
		// If there is search text, clear it and reload.
		private async Task OnAdornmentClick() {
			if (!string.IsNullOrWhiteSpace(_searchString))
				await ClearSearch();
		}
		// Clear search and reload
		private async Task ClearSearch() {
			_typingCts?.Cancel();
			_typingCts?.Dispose();
			_typingCts = null;

			_searchString = string.Empty;
			_appliedSearch = string.Empty;

			Console.WriteLine("[Client] Clearing search");

			await Task.Yield();
			await ReloadAllAsync(resetPage: false);
		}


		// Centralized reload logic used by ApplySearchAsync and ClearSearch
		private async Task ReloadAllAsync(bool resetPage) {
			if (resetPage && _grid is not null) {
				var setPageMethod = _grid.GetType().GetMethod("SetCurrentPageAsync", new[] { typeof(int) });
				if (setPageMethod is not null)
					await (Task)setPageMethod.Invoke(_grid, new object[] { 0 })!;
			}

			if (_grid is not null)
				await _grid.ReloadServerData();

			await GetProjectsMobileAsync(_activeSorting, _activeSortingDesc);

			StateHasChanged();
		}

		// Desktop: server-side data provider for MudDataGrid
		private async Task<GridData<ProjectDto.Index>> GetProjectsAsync(GridState<ProjectDto.Index> state) {
			var sorting = state.SortDefinitions.FirstOrDefault();
			var request = new QueryRequest.SkipTake {
				Skip = state.Page * state.PageSize,
				Take = state.PageSize,
				OrderBy = sorting?.SortBy ?? _activeSorting ?? "Name",
				OrderDescending = sorting?.Descending ?? _activeSortingDesc,
				SearchTerm = string.IsNullOrWhiteSpace(_appliedSearch) ? string.Empty : _appliedSearch.Trim()
			};
if (state.FilterDefinitions?.Any() == true)
{
	foreach (var f in state.FilterDefinitions)
	{
		var (key, value) = TryExtractFilter(f);
		if (!string.IsNullOrWhiteSpace(key) && value != null)
			request.Filters[key] = value;
	}
}

// lokale helper (onder de methode of bovenaan in dezelfde class)
(string? Key, object? Value) TryExtractFilter(object filterDef)
{
	object? GetPropValue(object? obj, params string[] names)
	{
		if (obj == null) return null;
		var t = obj.GetType();
		foreach (var n in names)
		{
			var p = t.GetProperty(n, BindingFlags.Public | BindingFlags.Instance);
			if (p != null) return p.GetValue(obj);
		}
		return null;
	}

	object? rawValue = GetPropValue(filterDef, "Value", "FilterValue", "Values", "Filter");
	// sommige implementaties hebben een Column-object met de info
	if (rawValue == null)
		rawValue = GetPropValue(GetPropValue(filterDef, "Column"), "Field", "Property", "PropertyName", "FieldName", "Name");

	// rawKey proberen uit directe props of uit een Column-object
	var rawKeyObj = GetPropValue(filterDef, "PropertyName", "Field", "MemberName", "ColumnName", "Name")
					?? GetPropValue(GetPropValue(filterDef, "Column"), "Field", "Property", "PropertyName", "FieldName", "Name");
	var rawKey = rawKeyObj?.ToString();

	// normaliseer key: "x => x.Code" of "p.Code" -> "Code"
	string? NormalizeKey(string? k)
	{
		if (string.IsNullOrWhiteSpace(k)) return null;
		k = k.Trim().Trim('"').Trim('\'');
		if (k.Contains("=>")) k = k.Split("=>").Last().Trim();
		if (k.Contains('.')) k = k.Split('.').Last().Trim();
		return k;
	}

	// als rawValue een enumerable is (niet string), pak eerste item
	object? FirstFromEnumerable(object? v)
	{
		if (v == null) return null;
		if (v is string) return v;
		if (v is IEnumerable ie)
		{
			foreach (var it in ie) return it;
			return null;
		}
		return v;
	}

	object? NormalizeValue(object? v)
	{
		if (v == null) return null;
		v = FirstFromEnumerable(v);

		if (v is JsonElement je)
		{
			return je.ValueKind switch
			{
				JsonValueKind.String => je.GetString(),
				JsonValueKind.Number => je.TryGetInt64(out var l) ? (object)l : je.TryGetDouble(out var d) ? (object)d : je.GetRawText(),
				JsonValueKind.True => true,
				JsonValueKind.False => false,
				JsonValueKind.Null => null,
				_ => je.GetRawText()
			};
		}

		return v;
	}

	return (NormalizeKey(rawKey), NormalizeValue(rawValue));
}

			Console.WriteLine($"[Client] GetProjectsAsync sending SearchTerm='{request.SearchTerm}', Skip={request.Skip}, Take={request.Take}");

			var result = await ProjectService.GetIndexAsync(request);

			return new GridData<ProjectDto.Index> {
				TotalItems = result.Value.TotalCount,
				Items = result.Value.Projects
			};
		}

		// Mobile: uses the same applied search
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

		// Mobile Sorting
		private void ToggleSortingDrawer() => _mobileSortOptionsOpen = !_mobileSortOptionsOpen;

		private async Task SortMobileAsync(string orderBy, bool descending) {
			_activeSorting = orderBy;
			_activeSortingDesc = descending;
			await GetProjectsMobileAsync(orderBy, descending);
			ToggleSortingDrawer();
		}

		// Navigation helpers
		private void NavigateToProject(Guid projectId) => NavigationManager.NavigateTo($"/projects/{projectId}");
		private void NavigateToCreate() => NavigationManager.NavigateTo("/projects/create");

		// Other
		private Color DefineColor(Status status) => status == Status.Actief ? Color.Success : Color.Error;
	}
}