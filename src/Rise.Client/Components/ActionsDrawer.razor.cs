using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

using MudBlazor;

namespace Rise.Client.Components;

// Code-behind for ActionsDrawer. Detects drawer close by intercepting the bound open value.
public partial class ActionsDrawer : ComponentBase, IDisposable {
	// Backing field used to store the true value without triggering the setter logic.
	private bool _openBackingField;
	[Inject] protected NavigationManager NavigationManager { get; set; } = null!;

	// Parent -> child Open parameter (supports @bind-Open from parent)
	[Parameter] public bool Open { get; set; }
	[Parameter] public EventCallback<bool> OpenChanged { get; set; }

	// Optional callback that parent can supply to be notified when drawer closes
	[Parameter] public EventCallback OnClose { get; set; }

	// Internal property bound to MudDrawer via @bind-Open.
	// Using a property lets us intercept changes: when the drawer closes (set to false)
	// we can run our close logic (propagate OpenChanged, call OnClose, update UI).
	private bool _open {
		get => _openBackingField;
		set {
			// If value didn't change, nothing to do
			if (_openBackingField == value) {
				return;
			}

			// Update backing field
			_openBackingField = value;

			// If drawer was closed, run the close handler.
			// We intentionally do not await here to keep setter synchronous;
			// instead we fire-and-forget the async handler.
			if (!_openBackingField) {
				_ = HandleCloseAsync();
			}
		}
	}

	// Anchor (right or bottom), set by the breakpoint provider
	private Anchor CurrentAnchor { get; set; } = Anchor.Right;

	private string ButtonText { get; set; } = "voeg project toe";
	private string NavigateUrl { get; set; } = "/projects/create";

	public void Dispose() {
		NavigationManager.LocationChanged -= OnLocationChanged;
	}

	protected override void OnInitialized() {
		base.OnInitialized();
		// Initialize button text / navigation target from current URI
		UpdateForUri(NavigationManager.Uri);
		// Subscribe to navigation changes so button text stays relevant
		NavigationManager.LocationChanged += OnLocationChanged;
		// Ensure our internal backing field matches initial parameter value
		_openBackingField = Open;
	}

	protected override void OnParametersSet() {
		// When the parent sets the Open parameter, update the backing field directly
		// (do NOT set the property _open here because that would trigger the setter logic).
		// This keeps the internal state in sync with the parent without treating parent-driven
		// updates as "close" events that need to be re-propagated.
		if (_openBackingField != Open) {
			_openBackingField = Open;
		}
	}

	private void OnLocationChanged(object? sender, LocationChangedEventArgs e) {
		UpdateForUri(e.Location);
		_ = InvokeAsync(StateHasChanged);
	}

	private void UpdateForUri(string uri) {
		if (uri.Contains("/projects", StringComparison.OrdinalIgnoreCase)) {
			NavigateUrl = "/projects/create";
			ButtonText = "voeg project toe"; // Dutch: "add project"
		} else if (uri.Contains("/stations", StringComparison.OrdinalIgnoreCase)) {
			NavigateUrl = "/stations/create";
			ButtonText = "voeg meetstation toe"; // Dutch: "add station"
		} else {
			// default
			NavigateUrl = "/projects/create";
			ButtonText = "voeg project toe";
		}
	}

	private void NavigateToCreate() {
		NavigationManager.NavigateTo(NavigateUrl);
	}

	// This async handler is invoked when the bound `_open` property becomes false
	// (i.e. when the user closed the MudDrawer by clicking outside or similar).
	// It performs the same propagate-and-notify logic you had before.
	private async Task HandleCloseAsync() {
		// Ensure the internal backing field is false
		_openBackingField = false;

		// Propagate two-way binding change to the parent if it provided a delegate.
		if (OpenChanged.HasDelegate) {
			await OpenChanged.InvokeAsync(false);
		}

		// Also call the optional OnClose callback if provided.
		if (OnClose.HasDelegate) {
			await OnClose.InvokeAsync(null);
		}

		// Ensure UI updates
		await InvokeAsync(StateHasChanged);
	}
}