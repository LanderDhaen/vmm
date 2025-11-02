using Microsoft.AspNetCore.Components;

using MudBlazor;

namespace Rise.Client.Components;

public partial class ArrowHeader : ComponentBase {
	[Parameter] public string Title { get; set; } = "";
	[Parameter] public EventCallback OnBack { get; set; }
}