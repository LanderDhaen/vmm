using Microsoft.AspNetCore.Components;

namespace Rise.Client.Layout {
	// Partial class for the ToolBar component (code-behind).
	public partial class ToolBar : ComponentBase {
		// Backing field for actions drawer only.
		private bool _actionsDrawerOpen;

		// Search and callbacks
		[Parameter] public string Search { get; set; } = string.Empty;
		[Parameter] public EventCallback<string> SearchChanged { get; set; }
		[Parameter] public EventCallback OnColumnsClicked { get; set; }
		[Parameter] public EventCallback OnFilterClicked { get; set; }
		[Parameter] public EventCallback OnActionsClicked { get; set; }

		// When toolbar wants to request the parent (page) to open the sort drawer, it invokes this.
		[Parameter] public EventCallback OnOpenSortRequested { get; set; }

		// Callback to inform the parent when the toolbar receives a sort selection (kept for compatibility).
		[Parameter] public EventCallback<string> OnSortChanged { get; set; }

		// Open the actions drawer (called by Actions / More icon)
		private async Task OpenActionsDrawer() {
			_actionsDrawerOpen = true;
			if (OnActionsClicked.HasDelegate)
				await OnActionsClicked.InvokeAsync();
		}

		// Request parent to open the sort drawer (called by Sort icon)
		private async Task RequestOpenSortDrawer() {
			if (OnOpenSortRequested.HasDelegate)
				await OnOpenSortRequested.InvokeAsync();
		}
	}
}