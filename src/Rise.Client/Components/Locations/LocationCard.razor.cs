using Microsoft.AspNetCore.Components;

using Rise.Shared.Locations;

namespace Rise.Client.Components.Locations;

public partial class LocationCard {
	[Parameter] public required LocationDto.Detail Location { get; init; }
	[Parameter] public bool ShowMoveButton { get; set; } = true;
	[Parameter] public EventCallback OnClick { get; set; }
}