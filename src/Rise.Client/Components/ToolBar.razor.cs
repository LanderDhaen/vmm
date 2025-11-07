using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;

using MudBlazor;

namespace Rise.Client.Layout {
	public partial class ToolBar : ComponentBase {
		// Search is initialized inline to ensure it's never null and supports two-way binding from parent
		[Parameter] public string Search { get; set; } = string.Empty;
		[Parameter] public EventCallback<string> SearchChanged { get; set; }

		// Callbacks for desktop actions (unchanged behaviour)
		[Parameter] public EventCallback OnColumnsClicked { get; set; }
		[Parameter] public EventCallback OnFilterClicked { get; set; }
		[Parameter] public EventCallback OnActionsClicked { get; set; }

		// Callback used to inform parent of sort change from the drawer (string: "" = reset; otherwise "Column:dir")
		[Parameter] public EventCallback<string> OnSortChanged { get; set; }

		// Internal drawer state
		private bool _actionsDrawerOpen = false;

		// Default columns passed to SortDrawer; parent may override by supplying its own Columns param on ToolBar if needed.

		// Open the drawer (called by mobile Sort icon)
		private void OpenActionsDrawer() {
			_actionsDrawerOpen = true;
		}

		// Close the drawer (called by drawer OnClose)
		private Task CloseActionsDrawer() {
			_actionsDrawerOpen = false;
			return Task.CompletedTask;
		}
	}
}