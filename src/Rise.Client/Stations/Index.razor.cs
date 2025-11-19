using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Services;
using Rise.Shared.Common;
using Rise.Shared.Stations;

namespace Rise.Client.Stations;

public partial class Index {
	private List<MeasurementStationDto.Index>? _mobileStations;
	private int _mobileTotal;
	[Inject] public required IMeasurementStationService StationService { get; set; }
	[Inject] public required BreadcrumbService BreadcrumbService { get; set; }
	[Inject] public required HeaderService HeaderService { get; set; }
	[Inject] public required NavigationManager NavigationManager { get; set; }

	protected override async Task OnInitializedAsync() {
		await base.OnInitializedAsync();
		await FetchMobileItemsAsync(); // uses default page = 0, pageSize = 10
		BreadcrumbService.SetBreadcrumbs(
			new BreadcrumbItem("Home", "/"),
			new BreadcrumbItem("Meetstations", null, true)
		);
		HeaderService.Set("Meetstations");
	}

	private async Task<GridData<MeasurementStationDto.Index>> GetStationsAsync(
		GridState<MeasurementStationDto.Index> state) {
		int skip = state.Page * state.PageSize;
		int take = state.PageSize;
		// Keep "Id" for default ordering to match previous behaviour; adapt if you map sorting later.
		Result<MeasurementStationResponse.Index>? response = await FetchStationsAsync(skip, take, "Id");

		if (response?.Value is not null) {
			return new GridData<MeasurementStationDto.Index> {
				TotalItems = response.Value.TotalCount,
				Items = response.Value.Stations
			};
		}

		// On error or empty response return an empty dataset so the grid can render.
		return new GridData<MeasurementStationDto.Index> {
			TotalItems = 0,
			Items = Array.Empty<MeasurementStationDto.Index>()
		};
	}

	private void OnRowClick(DataGridRowClickEventArgs<MeasurementStationDto.Index> args) {
		NavigationManager.NavigateTo($"/stations/{args.Item.Id}");
	}

	private static QueryRequest.SkipTake BuildRequest(int skip, int take, string? orderBy) {
		return new QueryRequest.SkipTake {
			Skip = skip,
			Take = take,
			OrderBy = orderBy
		};
	}

	// --- Centralized fetch used by both grid and mobile -----------------

	/// <summary>
	///     Centralized fetch that builds the request, calls the service and handles logging/errors.
	///     Returns null on exception so callers can handle fallback behaviour.
	/// </summary>
	private async Task<Result<MeasurementStationResponse.Index>?> FetchStationsAsync(int skip, int take,
		string? orderBy = null) {
		try {
			QueryRequest.SkipTake req = BuildRequest(skip, take, orderBy);
			Result<MeasurementStationResponse.Index> result = await StationService.GetIndexAsync(req);
			return result;
		} catch (Exception ex) {
			Log.Error(ex, "FetchStationsAsync failed (skip={Skip}, take={Take}, orderBy={OrderBy})", skip, take,
				orderBy);
			return null;
		}
	}

	// --- Mobile data (cards) --------------------------------------------

	// Default parameter values make fields unnecessary; call parameterless for page 0 / size 10
	private async Task FetchMobileItemsAsync(int page = 0, int pageSize = 10, string? orderBy = null) {
		Result<MeasurementStationResponse.Index>? response =
			await FetchStationsAsync(page * pageSize, pageSize, orderBy);

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
		if (id is null) {
			return;
		}

		NavigationManager.NavigateTo($"/stations/{id}");
	}
}