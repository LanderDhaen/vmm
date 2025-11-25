using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

using MudBlazor;

namespace Rise.Client.Layout;

public partial class NavMenu : ComponentBase {
	/// <summary>
	///     Navigation menu items. To add a new menu item, simply add a new tuple: (href, icon, label, match).
	/// </summary>
	private const NavLinkMatch Match = NavLinkMatch.Prefix;

	private static readonly List<(string Href, string Icon, string Label, NavLinkMatch Match)> MenuItems = [
		("/projects", Icons.Material.Filled.Folder, "Projecten", Match),
		("/stations", Icons.Material.Filled.Map, "Meetstations", Match),
		("/maintobjects", Icons.Material.Filled.ImportantDevices, "Onderhoudsobjecten", Match),
		("/workorders", Icons.Material.Filled.Archive, "Werkbonnen", Match),
		("/account", Icons.Material.Filled.Person, "Account", Match),
		("/settings", Icons.Material.Filled.Settings, "Instellingen", Match)
	];
}