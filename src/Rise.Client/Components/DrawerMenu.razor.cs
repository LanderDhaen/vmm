using MudBlazor;

namespace Rise.Client.Components;

public partial class DrawerMenu {
	private readonly List<NavItem> _items = new() {
		new NavItem("Projecten", "projects", Icons.Material.Filled.Folder),
		new NavItem("Meetstations", "stations", Icons.Material.Filled.Map),
		new NavItem("Apparaten", "devices", Icons.Material.Filled.ImportantDevices),
		new NavItem("Werkbonnen", "orders", Icons.Material.Filled.Archive),
		new NavItem("Account", "account", Icons.Material.Filled.AccountCircle)
	};

	private string GetHref(string slug) {
		return "/" + (slug ?? string.Empty).Trim().ToLowerInvariant();
	}

	private record NavItem(string Title, string Slug, string Icon);
}