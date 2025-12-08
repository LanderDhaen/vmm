using Microsoft.AspNetCore.Components;

using Rise.Shared.Stations;

namespace Rise.Client.Components.Stations;

public partial class StationCard : ComponentBase {
	[Inject] private NavigationManager Navigation { get; set; } = null!;

	[Parameter] public MeasurementStationDto.Detail? Station { get; init; }

	private void NavigateToMaintObj(Guid id) {
		Navigation.NavigateTo($"/stations/{id}");
	}
}