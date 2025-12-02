using Microsoft.AspNetCore.Components;
using Rise.Shared.Stations;

namespace Rise.Client.Stations;

public partial class History {
	private MeasurementStationDto.History? _measurementStation;
	
	[Parameter] public Guid MeasurementStationId { get; set; }
	[Inject] public required IMeasurementStationService MeasurementStationService { get; set; }
	

	protected override async Task OnInitializedAsync() {
		Result<MeasurementStationResponse.History> result =
			await MeasurementStationService.GetHistoryAsync(MeasurementStationId);

		if (result.IsSuccess) {
			_measurementStation = result.Value.MeasurementStation;
		}
	}
}