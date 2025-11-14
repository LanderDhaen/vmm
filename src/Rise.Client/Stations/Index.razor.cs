using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Services;
using Rise.Shared.Common;
using Rise.Shared.Stations;

using Serilog;

namespace Rise.Client.Stations;

public partial class Index {
	[Inject] public required StationService StationService { get; set; }
	[Inject] public required BreadcrumbService BreadcrumbService { get; set; }
	[Inject] public required HeaderService HeaderService { get; set; }

	protected override void OnInitialized() {
		BreadcrumbService.SetBreadcrumbs(
			new BreadcrumbItem("Home", "/"),
			new BreadcrumbItem("Meetstations", null, true)
		);
		HeaderService.Set("Meetstations");
	[Inject] public required NavigationManager NavigationManager { get; set; }

	private List<MeasurementStationDto.Index>? _mobileStations;
	private int _mobileTotal;

	protected override async Task OnInitializedAsync() {
		await base.OnInitializedAsync();
		await FetchMobileItemsAsync(); // uses default page = 0, pageSize = 10
	}

	private async Task<GridData<MeasurementStationDto.Index>> GetStationsAsync(
		GridState<MeasurementStationDto.Index> state) {

		int skip = state.Page * state.PageSize;
		int take = state.PageSize;
		// Keep "Id" for default ordering to match previous behaviour; adapt if you map sorting later.
		var response = await FetchStationsAsync(skip, take, "Id");

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

	private static QueryRequest.SkipTake BuildRequest(int skip, int take, string? orderBy)
		=> new() {
			Skip = skip,
			Take = take,
			OrderBy = orderBy,
		};

	// --- Centralized fetch used by both grid and mobile -----------------

	/// <summary>
	/// Centralized fetch that builds the request, calls the service and handles logging/errors.
	/// Returns null on exception so callers can handle fallback behaviour.
	/// </summary>
	private async Task<Result<MeasurementStationResponse.Index>?> FetchStationsAsync(int skip, int take, string? orderBy = null) {
		try {
			var req = BuildRequest(skip, take, orderBy);
			var result = await StationService.GetIndexAsync(req);
			return result;
		} catch (Exception ex) {
			Log.Error(ex, "FetchStationsAsync failed (skip={Skip}, take={Take}, orderBy={OrderBy})", skip, take, orderBy);
			return null;
		}
	}

	// --- Mobile data (cards) --------------------------------------------

	// Default parameter values make fields unnecessary; call parameterless for page 0 / size 10
	private async Task FetchMobileItemsAsync(int page = 0, int pageSize = 10, string? orderBy = null) {
		var response = await FetchStationsAsync(page * pageSize, pageSize, orderBy);

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
		if (string.IsNullOrWhiteSpace(raw))
			return string.Empty;

		return raw switch {
			"Active" or "Actief" => "Actief",
			"Inactive" or "Inactief" => "Inactief",
			_ => raw
		};
	}
}