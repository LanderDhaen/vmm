using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Services;
using Rise.Shared.Common;
using Rise.Shared.Stations;

namespace Rise.Client.Stations {
	public partial class Index {
		// ----- refs & services ------------------------------------------------
		private MudDataGrid<MeasurementStationDto.Index>? _grid;

		[Inject] public StationService StationService { get; set; } = null!;
		[Inject] public NavigationManager NavigationManager { get; set; } = null!;
		[Inject] public BreadcrumbService BreadcrumbService { get; set; } = null!;
		[Inject] public HeaderService HeaderService { get; set; } = null!;

		// ----- mobile list state ---------------------------------------------
		private List<MeasurementStationDto.Index>? _mobileStations;
		private int _mobileTotal;
		private const int MobilePage = 0;
		private const int MobilePageSize = 10;

		// ----- mobile sort override ------------------------------------------
		private string? _mobileSortBy;
		private bool _mobileSortDescending;
		public bool Open { get; set; }
		public List<(string Value, string Label)> Columns { get; set; } = new();

		// ----- lifecycle -----------------------------------------------------
		protected override async Task OnInitializedAsync() {
			await base.OnInitializedAsync();

			BreadcrumbService.SetBreadcrumbs(
				new BreadcrumbItem("Home", "/"),
				new BreadcrumbItem("Meetstations", null, true)
			);
			HeaderService.Set("Meetstations");

			InitColumnsFromDto();
			await FetchMobileItemsAsync(); // load first mobile page (respects mobile sort)
		}

		// ----- grid server data provider ------------------------------------
		private async Task<GridData<MeasurementStationDto.Index>> GetStationsAsync(GridState<MeasurementStationDto.Index> state) {
			var skip = state.Page * state.PageSize;
			var take = state.PageSize;
			var (orderBy, orderDesc) = ResolveOrder(state);

			var result = await StationService.GetIndexAsync(BuildRequest(skip, take, orderBy, orderDesc));

			if (result?.Value is not null) {
				return new GridData<MeasurementStationDto.Index> {
					TotalItems = result.Value.TotalCount,
					Items = result.Value.Stations ?? Array.Empty<MeasurementStationDto.Index>()
				};
			}

			return new GridData<MeasurementStationDto.Index> {
				TotalItems = 0,
				Items = Array.Empty<MeasurementStationDto.Index>()
			};
		}

		private void OnRowClick(DataGridRowClickEventArgs<MeasurementStationDto.Index> args) =>
			NavigationManager.NavigateTo($"/stations/{args.Item.Id}");

		// ----- helpers -------------------------------------------------------
		private static QueryRequest.SkipTake BuildRequest(int skip, int take, string? orderBy, bool orderDescending = false) =>
			new() { Skip = skip, Take = take, OrderBy = orderBy, OrderDescending = orderDescending };

		private (string? orderBy, bool orderDesc) ResolveOrder(GridState<MeasurementStationDto.Index> state) {
			if (!string.IsNullOrEmpty(_mobileSortBy)) return (_mobileSortBy, _mobileSortDescending);

			var s = state.SortDefinitions.FirstOrDefault();
			return (s?.SortBy, s?.Descending ?? false);
		}

		// ----- sorting integration -------------------------------------------
		private void ApplySortSelection(string selection) {
			if (string.IsNullOrWhiteSpace(selection)) {
				_mobileSortBy = null;
				_mobileSortDescending = false;
			} else {
				var parts = selection.Split(new[] { ':', '|' }, StringSplitOptions.RemoveEmptyEntries)
									  .Select(p => p.Trim()).ToArray();
				_mobileSortBy = parts.ElementAtOrDefault(0);
				_mobileSortDescending = string.Equals(parts.ElementAtOrDefault(1), "desc", StringComparison.OrdinalIgnoreCase);
			}

			Open = false;
		}

		private async Task ApplySortAndClose(string column, string direction) {
			var selection = string.IsNullOrEmpty(column) ? string.Empty : $"{column}:{direction}";
			ApplySortSelection(selection);

			await FetchMobileItemsAsync();
			if (_grid is not null) await _grid.ReloadServerData();

			await InvokeAsync(StateHasChanged);
		}

		private async Task HandleSortChanged(string selection) {
			ApplySortSelection(selection);
			await FetchMobileItemsAsync();
			if (_grid is not null) await _grid.ReloadServerData();
			await InvokeAsync(StateHasChanged);
		}

		private Color GetButtonColor(string column, string direction) {
			if (string.IsNullOrEmpty(_mobileSortBy)) return Color.Default;

			return (_mobileSortBy == column && ((direction == "desc") == _mobileSortDescending))
				? Color.Primary
				: Color.Default;
		}

		// ----- centralized fetch used by both grid and mobile -----------------

		private async Task<Result<MeasurementStationResponse.Index>?> FetchStationsAsync(int skip, int take, string? orderBy = null, bool orderDesc = false) {
			try {
				var req = BuildRequest(skip, take, orderBy, orderDesc);
				var result = await StationService.GetIndexAsync(req);
				return result;
			} catch (Exception ex) {
				Log.Error(ex, "FetchStationsAsync failed (skip={Skip}, take={Take}, orderBy={OrderBy}, orderDesc={OrderDesc})", skip, take, orderBy, orderDesc);
				return null;
			}
		}

		// ----- Mobile data (cards) --------------------------------------------

		private async Task FetchMobileItemsAsync(int page = MobilePage, int pageSize = MobilePageSize) {
			var reqOrderBy = _mobileSortBy;
			var reqOrderDesc = _mobileSortDescending;

			var response = await FetchStationsAsync(page * pageSize, pageSize, reqOrderBy, reqOrderDesc);

			if (response?.Value is not null) {
				_mobileStations = response.Value.Stations?.ToList() ?? new List<MeasurementStationDto.Index>();
				_mobileTotal = response.Value.TotalCount;
			} else {
				Log.Warning("FetchMobileItemsAsync: service returned null or empty result");
				_mobileStations = new List<MeasurementStationDto.Index>();
				_mobileTotal = 0;
			}

			await InvokeAsync(StateHasChanged);
		}

		private static string TranslateStatus(string raw) {
			if (string.IsNullOrWhiteSpace(raw)) {
				return string.Empty;
			}

			return raw switch {
				"Active" or "Actief" => "Actief",
				"Inactive" or "Inactief" => "Inactief",
				_ => raw
			};
		}

		private void NavigateToStation(object? id) {
			if (id is null) return;
			NavigationManager.NavigateTo($"/stations/{id}");
		}

		// ----- column label helpers -----------------------------------------
		private void InitColumnsFromDto() {
			if (Columns != null && Columns.Any()) return;

			var desiredOrder = new[] { "Code", "Status", "InstalledAt", "RemovedAt", "Location", "City", "ProjectsCount" };

			var excluded = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "Id" };

			var props = typeof(MeasurementStationDto.Index)
				.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(p => !excluded.Contains(p.Name))
				.ToDictionary(p => p.Name, StringComparer.OrdinalIgnoreCase);

			var list = new List<(string Value, string Label)>(desiredOrder.Length);

			foreach (var name in desiredOrder) {
				if (props.TryGetValue(name, out var prop)) {
					list.Add((Value: prop.Name, Label: GetDisplayName(prop.Name)));
				}
			}

			foreach (var prop in props.Values.OrderBy(p => p.Name)) {
				if (!list.Any(x => string.Equals(x.Value, prop.Name, StringComparison.OrdinalIgnoreCase))) {
					list.Add((Value: prop.Name, Label: GetDisplayName(prop.Name)));
				}
			}

			Columns = list;
		}

		private static string GetDisplayName(string propertyName) {
			var prop = typeof(MeasurementStationDto.Index).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
			if (prop is null) return propertyName;

			var disp = Attribute.GetCustomAttribute(prop, typeof(DisplayAttribute)) as DisplayAttribute;
			return disp?.GetName() ?? propertyName;
		}
	}

	internal static class _StringExtensions {
		public static bool HasValue(this string? s) => !string.IsNullOrWhiteSpace(s);
	}
}