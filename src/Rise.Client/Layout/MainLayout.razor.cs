using Microsoft.AspNetCore.Components;

using MudBlazor;
using MudBlazor.Utilities;

namespace Rise.Client.Layout;

public partial class MainLayout : LayoutComponentBase {
	private bool _open;

	// Placeholder property (the Razor markup uses ShowDrawer).
	private bool ShowDrawer => true;

	[Inject] private NavigationManager NavigationManager { get; set; } = default!;

	protected override void OnInitialized() {
		// Subscribe to location changes so we can close the mobile drawer after navigation.
		// Use a handler with the exact expected signature.
		NavigationManager.LocationChanged += HandleLocationChanged;
	}

	// Exact match for EventHandler<LocationChangedEventArgs>
	private void HandleLocationChanged(object sender, Microsoft.AspNetCore.Components.Routing.LocationChangedEventArgs e) {
		if (_open) {
			_open = false;
			// Ensure StateHasChanged runs on the renderer/UI thread
			InvokeAsync(StateHasChanged);
		}
	}

	// Toggle is invoked directly from the MudIconButton.
	public Task ToggleDrawerAsync() {
		_open = !_open;
		InvokeAsync(StateHasChanged);
		return Task.CompletedTask;
	}

	// Explicit Close method (can be called by child components if they expose an EventCallback)
	public Task CloseDrawerAsync() {
		if (_open) {
			_open = false;
			InvokeAsync(StateHasChanged);
		}
		return Task.CompletedTask;
	}

	// MudDrawer calls OnClose when the drawer is closed (e.g., clicking outside).
	private Task OnDrawerClosed() {
		if (_open) {
			_open = false;
			InvokeAsync(StateHasChanged);
		}

		return Task.CompletedTask;
	}

	public void Dispose() {
		// Unsubscribe to avoid memory leaks / duplicate handlers
		NavigationManager.LocationChanged -= HandleLocationChanged;
	}

	private readonly MudTheme _vmm = new() {
		PaletteLight = new PaletteLight {
			Primary = new MudColor("#126B8D") // Matches color from the VMM logo
		}
	};
}