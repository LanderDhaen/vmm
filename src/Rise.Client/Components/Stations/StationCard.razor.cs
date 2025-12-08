using Microsoft.AspNetCore.Components;

using Rise.Shared.Stations;

namespace Rise.Client.Components.Stations;

public partial class StationCard : ComponentBase {
	[Parameter] public required MeasurementStationDto.Index MeasurementStation { get; init; }
}