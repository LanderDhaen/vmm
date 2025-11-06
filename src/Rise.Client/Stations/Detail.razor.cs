using Microsoft.AspNetCore.Components;
using MudBlazor;
using Rise.Shared.Common;
using Rise.Shared.Stations;

namespace Rise.Client.Stations;

public partial class Detail
{
	// ===== Breadcrumbs =====
	private readonly List<BreadcrumbItem> _items = new();

	// ===== Tabs =====
	private readonly List<string> _tabs = new()
	{
		"Info",
		"Apparaten",   
		"Historiek"
	};

	// ===== Main =====
	private MeasurementStationDto.Detail? _station;
	private int _selectedTab;
	private bool _loaded;

	[Parameter] public Guid StationId { get; set; }

	[Inject] public required StationService StationService { get; set; }

	private void SetTab(int index) => _selectedTab = index;

	protected override async Task OnInitializedAsync()
	{
		Result<MeasurementStationResponse.Detail> result = await StationService.GetDetailAsync(StationId);
		_loaded = true;

		if (result.IsSuccess)
		{
			_station = result.Value.Station;

			_items.Clear();
			_items.Add(new BreadcrumbItem("", "/", icon: Icons.Material.Filled.Home));
			_items.Add(new BreadcrumbItem("Meetstations", "/stations"));
			_items.Add(new BreadcrumbItem(_station.Code, null, true));
		}
	}

	private void NavigateToStations() => Navigation.NavigateTo("/stations/");

	private static string FormatDate(DateTime? dt)
		=> dt is null ? "-" : dt.Value.ToShortDateString();

	private static string FormatStatus(string? raw)
	{
		return raw switch
		{
			"Active" or "Actief"     => "Actief",
			"Inactive" or "Inactief" => "Inactief",
			_                        => raw ?? "-"
		};
	}
}