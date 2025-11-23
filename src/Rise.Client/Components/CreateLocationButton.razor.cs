using Microsoft.AspNetCore.Components;

namespace Rise.Client.Components;

public partial class CreateLocationButton {
	[Parameter] public EventCallback OnClick { get; set; }
}