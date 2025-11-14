using Microsoft.AspNetCore.Components;

using MudBlazor;
using MudBlazor.Utilities;

namespace Rise.Client.Layout;

public partial class MainLayout : LayoutComponentBase {
	private readonly bool _overlayAutoClose = true;

	private readonly MudTheme _vmm = new() {
		PaletteLight = new PaletteLight {
			Primary = new MudColor("#126B8D")
		}
	};

	private bool _open;

	private void ToggleDrawer() {
		_open = !_open;
	}

	private void BreadcrumbsChanged() {
		InvokeAsync(StateHasChanged);
	}
}