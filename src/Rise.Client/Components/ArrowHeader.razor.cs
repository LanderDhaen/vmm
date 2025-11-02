using Microsoft.AspNetCore.Components;

using MudBlazor;

namespace Rise.Client.Components;

public partial class ArrowHeader : ComponentBase {
	[Parameter] public string Title { get; set; } = "";
	[Parameter] public EventCallback OnBack { get; set; }
	[Parameter] public string Class { get; set; } = "m-1";
	[Parameter] public string FontWeight { get; set; } = "800";
	[Parameter] public Typo TitleTypo { get; set; } = Typo.h3;
}