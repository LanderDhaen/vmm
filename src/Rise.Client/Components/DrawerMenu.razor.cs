using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;

using MudBlazor;

namespace Rise.Client.Components {
	public partial class DrawerMenu : ComponentBase {
		// Parent can pass this callback: <Rise.Client.Components.DrawerMenu OnNavigate="CloseDrawerAsync" />
		[Parameter] public EventCallback OnNavigate { get; set; }


		// The navigation items
		private record NavItem(string Title, string Href, string Icon);

		private readonly List<NavItem> _items = new()
		{
	new NavItem("Projecten", "/projects", Icons.Material.Filled.Folder),
	new NavItem("Meetstations", "/stations", Icons.Material.Filled.Map),
	new NavItem("Apparaten", "/devices", Icons.Material.Filled.ImportantDevices),
	new NavItem("Werkbonnen", "/orders", Icons.Material.Filled.Archive),
	new NavItem("Account", "/account", Icons.Material.Filled.AccountCircle)
};

	}
}