using System;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

using MudBlazor;

namespace Rise.Client.Layout {
	public partial class ActionsDrawer : ComponentBase, IDisposable {
		[Inject] protected NavigationManager NavigationManager { get; set; } = null!;

		// External binding parameters (support for parent bind)
		[Parameter] public bool Open { get; set; }
		[Parameter] public EventCallback<bool> OpenChanged { get; set; }

		// Optional callback the parent passed to be notified when the drawer closes
		[Parameter] public EventCallback OnClose { get; set; }

		// Internal state to bind to MudDrawer
		private bool _open { get; set; }

		private Anchor CurrentAnchor { get; set; } = Anchor.Right;

		private string ButtonText { get; set; } = "voeg project toe";
		private string NavigateUrl { get; set; } = "/projects/create";

		protected override void OnInitialized() {
			base.OnInitialized();
			UpdateForUri(NavigationManager.Uri);
			NavigationManager.LocationChanged += OnLocationChanged;
		}

		// Ensure internal state is synchronized with the parent parameter
		protected override void OnParametersSet() {
			// Only update if the parent's value differs
			if (_open != Open)
				_open = Open;
		}

		private void OnLocationChanged(object? sender, LocationChangedEventArgs e) {
			UpdateForUri(e.Location);
			_ = InvokeAsync(StateHasChanged);
		}

		private void UpdateForUri(string uri) {
			if (uri.Contains("/projects", StringComparison.OrdinalIgnoreCase)) {
				NavigateUrl = "/projects/create";
				ButtonText = "voeg project toe";
			} else if (uri.Contains("/stations", StringComparison.OrdinalIgnoreCase)) {
				NavigateUrl = "/stations/create";
				ButtonText = "voeg meetstation toe";
			} else {
				NavigateUrl = "/projects/create";
				ButtonText = "voeg project toe";
			}
		}

		private Task NavigateToCreate() {
			NavigationManager.NavigateTo(NavigateUrl);
			return Task.CompletedTask;
		}

		// MudDrawer OnClose handler — update internal state and propagate to parent
		private async Task HandleClose() {
			_open = false;

			// Propagate two-way binding change if parent provided OpenChanged
			if (OpenChanged.HasDelegate)
				await OpenChanged.InvokeAsync(false);

			// Also call the OnClose callback if provided
			if (OnClose.HasDelegate)
				await OnClose.InvokeAsync(null);

			// Ensure UI updates
			await InvokeAsync(StateHasChanged);
		}

		public void Dispose() {
			NavigationManager.LocationChanged -= OnLocationChanged;
		}
	}
}