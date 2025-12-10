using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.Stations;

namespace Rise.Client.Stations;

public partial class History {
	private IEnumerable<MeasurementStationDto.Modification>? _modifications;
	[Parameter] public Guid MeasurementStationId { get; init; }
	[Inject] public required IMeasurementStationService MeasurementStationService { get; init; }
	
	protected override async Task OnInitializedAsync() {
		Result<MeasurementStationResponse.History> result =
			await MeasurementStationService.GetHistoryAsync(MeasurementStationId);

		if (result.IsSuccess) {
			_modifications = result.Value.Modifications;
		}
	}
}