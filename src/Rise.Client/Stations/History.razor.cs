using Microsoft.AspNetCore.Components;
using Rise.Shared.Stations;

namespace Rise.Client.Stations;

public partial class History {
	private IEnumerable<MeasurementStationDto.Modification>? _modifications;
	
	[Parameter] public Guid MeasurementStationId { get; set; }
	[Inject] public required IMeasurementStationService MeasurementStationService { get; set; }
	

	protected override async Task OnInitializedAsync() {
		Result<MeasurementStationResponse.History> result =
			await MeasurementStationService.GetHistoryAsync(MeasurementStationId);

		if (result.IsSuccess) {
			_modifications = result.Value.Modifications;
		}
	}
}