using Microsoft.AspNetCore.Components;

namespace Rise.Client.Layout;

// Partial class for the ToolBar component (code-behind).
public partial class ToolBar : ComponentBase {
	// Internal boolean bound to ActionsDrawer via @bind-Open.
	// When this is true, the drawer opens. When false, it closes.
	private bool _actionsDrawerOpen;

	// Search parameter with two-way binding support
	[Parameter] public string Search { get; set; } = string.Empty;
	[Parameter] public EventCallback<string> SearchChanged { get; set; }

	// These are callbacks the parent of ToolBar could pass in.
	[Parameter] public EventCallback OnColumnsClicked { get; set; }
	[Parameter] public EventCallback OnFilterClicked { get; set; }
	[Parameter] public EventCallback OnActionsClicked { get; set; }
	[Parameter] public EventCallback OnSortClicked { get; set; }
	[Parameter] public EventCallback<string> OnSortChanged { get; set; }

	// Called from the toolbar UI when the user requests "Acties" / More actions.
	// Setting this to true causes the bound ActionsDrawer's _open to become true,
	// which causes the MudDrawer to display.
	private void OpenActionsDrawer() {
		_actionsDrawerOpen = true;
	}

	private async Task HandleSortClickAsync() {
		if (OnSortClicked.HasDelegate) {
			await OnSortClicked.InvokeAsync();
		}
	}
}