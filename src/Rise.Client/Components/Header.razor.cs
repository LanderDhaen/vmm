using Microsoft.AspNetCore.Components;

namespace Rise.Client.Components;

public partial class Header : ComponentBase {
	[Parameter] public string Title { get; set; } = "";
	[Parameter] public EventCallback OnBack { get; set; }
	[Parameter] public string? Icon { get; set; }
}