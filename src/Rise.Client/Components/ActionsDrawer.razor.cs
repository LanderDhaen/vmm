using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;

namespace Rise.Client.Layout {
	public partial class ActionsDrawer : ComponentBase {
		[Parameter] public bool Open { get; set; }
		[Parameter] public EventCallback<bool> OpenChanged { get; set; } // voor two-way binding
		[Parameter] public EventCallback OnClose { get; set; }

		private async Task CloseAsync() {
			// Update parent via OpenChanged zodat parent-state synchroon blijft
			if (OpenChanged.HasDelegate) {
				await OpenChanged.InvokeAsync(false);
			} else {
				// fallback: update lokaal zodat UI reageert als parent geen bind levert
				Open = false;
			}

			// optionele extra callback naar parent
			if (OnClose.HasDelegate)
				await OnClose.InvokeAsync();
		}

		private async Task HandleClose() {
			await CloseAsync();
		}

		// Nieuwe methode: sluit de drawer en navigeert naar /projects/create
		private async Task NavigateToCreate() {
			// sluit de drawer (houdt parent-state synchroon)
			await CloseAsync();

			// navigeer naar de create-pagina
			NavigationManager.NavigateTo("/projects/create");
		}
	}
}