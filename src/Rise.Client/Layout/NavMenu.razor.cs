using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

using MudBlazor;

namespace Rise.Client.Layout;

public partial class NavMenu : ComponentBase {
	/// <summary>
	///     Navigation menu items. To add a new menu item, simply add a new tuple: (href, icon, label, match).
	/// </summary>

	// TODO: configure other navs
	private readonly List<(string Href, string Icon, string Label, NavLinkMatch Match)> _menuItems = [
		("/projects", Icons.Material.Filled.Folder, "Projecten", NavLinkMatch.Prefix),
		("/stations", Icons.Material.Filled.Map, "Meetstations", NavLinkMatch.All),
		("/apparaten", Icons.Material.Filled.ImportantDevices, "Apparaten", NavLinkMatch.All),
		("/werkbonnen", Icons.Material.Filled.Archive, "Werkbonnen", NavLinkMatch.All),
		("/account", Icons.Material.Filled.Person, "Account", NavLinkMatch.All),
		("/instellingen", Icons.Material.Filled.Settings, "Instellingen", NavLinkMatch.All)
	];
}