using Microsoft.AspNetCore.Components;

using MudBlazor;
using MudBlazor.Utilities;

namespace Rise.Client.Layout {
	public partial class MainLayout : LayoutComponentBase {

		readonly MudTheme _vmm = new() {
			PaletteLight = new PaletteLight() {
				Primary = new MudColor("#126B8D"), // Matches color from the VMM logo
			},
		};
	}
}