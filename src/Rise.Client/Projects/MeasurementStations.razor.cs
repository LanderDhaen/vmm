using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.Common;
using Rise.Shared.Projects;
using Rise.Shared.Stations;

namespace Rise.Client.Projects;

public partial class MeasurementStations {
	[Parameter] public Guid ProjectId { get; set; }
	[Parameter] public IEnumerable<MeasurementStationDto.Index>? Stations { get; set; }
	[Inject] public required NavigationManager Navigation { get; set; }

	private void OnStationRowClick(TableRowClickEventArgs<MeasurementStationDto.Index> args) {
		Navigation.NavigateTo($"/stations/{args.Item!.Id}");
	}

	private void OnStationCardClick(MeasurementStationDto.Index station) {
		Navigation.NavigateTo($"/stations/{station.Id}");
	}
}