using Microsoft.AspNetCore.Components;

using Rise.Shared.Stations;

namespace Rise.Client.Components.Stations;

public partial class CreateStationForm {
	[Parameter] public required MeasurementStationRequest.Create Model { get; set; }
}