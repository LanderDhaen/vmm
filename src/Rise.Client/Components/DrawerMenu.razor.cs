using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;

using MudBlazor;

namespace Rise.Client.Components {
	public partial class DrawerMenu : ComponentBase {
		// Parent can pass this callback: <Rise.Client.Components.DrawerMenu OnNavigate="CloseDrawerAsync" />
		[Parameter] public EventCallback OnNavigate { get; set; }

		[Inject] private NavigationManager NavigationManager { get; set; } = default!;

		// The navigation items
		private readonly List<NavItem> _items = new()
		{
			new NavItem("Projecten", "projects", Icons.Material.Filled.Folder),
			new NavItem("Meetstations", "stations", Icons.Material.Filled.Map),
			new NavItem("Apparaten", "devices", Icons.Material.Filled.ImportantDevices),
			new NavItem("Werkbonnen", "orders", Icons.Material.Filled.Archive),
			new NavItem("Account", "account", Icons.Material.Filled.AccountCircle)
		};

		private string GetHref(string slug) => "/" + (slug ?? string.Empty).Trim().ToLowerInvariant();

		private async Task NavigateToAsync(string href) {
			// Programmatic navigation so we can call OnNavigate afterwards.
			NavigationManager.NavigateTo(href);

			// Invoke the parent callback to close the drawer (if provided).
			if (OnNavigate.HasDelegate)
				await OnNavigate.InvokeAsync(null);
		}

		private record NavItem(string Title, string Slug, string Icon);
	}
}