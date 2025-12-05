using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.Stations;

namespace Rise.Client.Stations;

public partial class History {
	private IEnumerable<MeasurementStationDto.Modification>? _modifications;
	[Parameter] public Guid MeasurementStationId { get; set; }
	[Inject] public required IMeasurementStationService MeasurementStationService { get; set; }
	
	private TimelinePosition _timelinePosition;
	

	protected override async Task OnInitializedAsync() {
		Result<MeasurementStationResponse.History> result =
			await MeasurementStationService.GetHistoryAsync(MeasurementStationId);

		if (result.IsSuccess) {
			_modifications = result.Value.Modifications;
		}
	}

	private void OnBreakPointChange(Breakpoint breakpoint) => _timelinePosition = breakpoint == Breakpoint.Xs ? TimelinePosition.Left : TimelinePosition.Alternate;
	
	
	private static string DefineTitle(MeasurementStationModificationType modificationType)
	{
		return modificationType switch
		{
			MeasurementStationModificationType.LocationChanged => "Locatie gewijzigd",
			MeasurementStationModificationType.InstalledAtChanged => "Installatiedatum gewijzigd",
			MeasurementStationModificationType.RemovedAtChanged => "Verwijderdatum gewijzigd",
			_ => "Wijziging"
		};
	}

}