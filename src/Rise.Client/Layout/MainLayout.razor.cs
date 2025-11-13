using MudBlazor;

namespace Rise.Client.Layout;

public partial class MainLayout {
	private readonly MudTheme _vmm = new() {
		PaletteLight = new PaletteLight {
			Primary = "#126B8D",
			TextSecondary = "#126B8D"
		}
	};
}