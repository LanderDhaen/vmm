using Microsoft.AspNetCore.Components;

using Rise.Shared.Stations;

namespace Rise.Client.Components.Stations;

public partial class MeasurementStationHistoryTimeline {
	[Parameter] public required MeasurementStationDto.History MeasurementStation { get; init; }
}