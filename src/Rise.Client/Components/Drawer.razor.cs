using Microsoft.AspNetCore.Components;

namespace Rise.Client.Layout;

public partial class Drawer : LayoutComponentBase {
	// Drawer state
	private bool _open;
	[Parameter] public RenderFragment? ChildContent { get; set; }

	// Placeholder property (the Razor markup uses ShowDrawer).
	private bool ShowDrawer => true;

	// Toggle is invoked directly from the MudIconButton.
	public Task ToggleDrawerAsync() {
		_open = !_open;
		// Force re-render
		StateHasChanged();
		return Task.CompletedTask;
	}

	// MudDrawer calls OnClose when the drawer is closed (e.g., clicking outside).
	private Task OnDrawerClosed() {
		if (_open) {
			_open = false;
			StateHasChanged();
		}

		return Task.CompletedTask;
	}
}