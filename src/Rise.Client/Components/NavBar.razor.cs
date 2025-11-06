using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

using MudBlazor;

namespace Rise.Client.Layout {
	// NavBar code-behind: builds breadcrumbs and exposes OnToggleDrawer callback.
	// English comments are used throughout as requested.
	public partial class NavBar : LayoutComponentBase, IDisposable {
		[Inject] protected NavigationManager NavigationManager { get; set; } = default!;

		// EventCallback provided by the parent to toggle the drawer (menu)
		[Parameter] public EventCallback OnToggleDrawer { get; set; }

		// Breadcrumb items for MudBreadcrumbs (MudBlazor expects List<BreadcrumbItem>)
		private List<BreadcrumbItem> _breadcrumbs = new();

		// Translation dictionary for known route segments -> localized display text.
		// English comments: keep translations here to ensure labels like "projects" become "Projecten".
		private static readonly Dictionary<string, string> _segmentTranslations = new(StringComparer.OrdinalIgnoreCase)
		{
			{ "projects", "Projecten" },
			{ "projecten", "Projecten" }, // tolerate Dutch variant too
			{ "stations", "Meetstations" },
			{ "devices", "Apparaten" },
			{ "orders", "Werkbonnen" },
			{ "account", "Account" },
			// add additional mappings here as needed
		};

		protected override void OnInitialized() {
			base.OnInitialized();
			// Subscribe to navigation changes so breadcrumbs update when route changes.
			NavigationManager.LocationChanged += OnLocationChanged;
			BuildBreadcrumbs();
		}

		private void OnLocationChanged(object? sender, LocationChangedEventArgs e) {
			BuildBreadcrumbs();
			InvokeAsync(StateHasChanged);
		}

		// Build breadcrumbs based on the current base-relative path.
		// Uses MapSegmentText to translate known route segments to localized text.
		private void BuildBreadcrumbs() {
			try {
				var baseRel = NavigationManager.ToBaseRelativePath(NavigationManager.Uri) ?? string.Empty;

				// strip query string safely
				var qIndex = baseRel.IndexOf('?');
				var path = qIndex >= 0 ? baseRel.Substring(0, qIndex) : baseRel;

				path = path.Trim('/');

				var segments = string.IsNullOrEmpty(path)
					? Array.Empty<string>()
					: path.Split('/', StringSplitOptions.RemoveEmptyEntries)
						  .Where(s => !string.IsNullOrWhiteSpace(s))
						  .ToArray();

				var items = new List<BreadcrumbItem>();

				// Root item "VMM" (home). Disabled if already on root.
				var rootDisabled = segments.Length == 0;
				items.Add(new BreadcrumbItem("VMM", href: "/", disabled: rootDisabled, icon: Icons.Material.Outlined.Home));

				// Build cumulative hrefs for each cleaned segment
				var cumulative = "";
				for (int i = 0; i < segments.Length; i++) {
					var seg = segments[i];
					cumulative += "/" + seg;

					// Last segment is disabled (current page)
					var disabled = (i == segments.Length - 1);

					// Map segment to human-friendly / localized text
					var text = MapSegmentText(seg);

					items.Add(new BreadcrumbItem(text, href: disabled ? null : cumulative, disabled: disabled));
				}

				_breadcrumbs = items;
			} catch {
				// Fallback breadcrumbs: use the same mapping function to ensure consistent labels.
				_breadcrumbs = new List<BreadcrumbItem>
				{
					new BreadcrumbItem("VMM", href: "/", disabled: false, icon: Icons.Material.Outlined.Home),
					new BreadcrumbItem(MapSegmentText("projects"), href: null, disabled: true)
				};
			}
		}

		// MapSegmentText: try dictionary translation first, fallback to humanization.
		private static string MapSegmentText(string seg) {
			if (string.IsNullOrWhiteSpace(seg))
				return seg;

			if (_segmentTranslations.TryGetValue(seg, out var mapped))
				return mapped;

			// fallback: humanize (replace -/_ with spaces and TitleCase)
			return HumanizeSegment(seg);
		}

		// HumanizeSegment: simple fallback humanization for unknown segments.
		// Example: "project-details" -> "Project Details"
		private static string HumanizeSegment(string seg) {
			if (string.IsNullOrWhiteSpace(seg))
				return seg;

			var replaced = seg.Replace('-', ' ').Replace('_', ' ');
			var words = replaced.Split(' ', StringSplitOptions.RemoveEmptyEntries)
								.Select(w => char.ToUpperInvariant(w[0]) + (w.Length > 1 ? w.Substring(1) : string.Empty));
			return string.Join(' ', words);
		}

		// Cleanup subscription when component is disposed.
		public void Dispose() => NavigationManager.LocationChanged -= OnLocationChanged;
	}
}