using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

using MudBlazor;

namespace Rise.Client.Layout {
	public partial class Drawer : LayoutComponentBase, IDisposable {
		[Parameter] public RenderFragment? ChildContent { get; set; }
		[Inject] protected NavigationManager NavigationManager { get; set; } = default!;
		[Inject] protected IJSRuntime JSRuntime { get; set; } = default!;
		[Inject] protected ILogger<Drawer> Logger { get; set; } = default!;

		// Drawer state
		private bool _open = false;
		private const int LargeScreenThreshold = 960;

		// Breadcrumbs: list of (Text, Href)
		private readonly List<(string Text, string Href)> _breadcrumbs = new();

		// cached last known "is large" state to use in render
		private bool _isLarge;

		// ShowDrawer: determines whether the drawer should be shown based on breadcrumbs (homepage => only "Home")
		// We avoid an explicit IsHome boolean - the decision is derived from the breadcrumb-like structure.
		private bool ShowDrawer => _breadcrumbs.Count > 1;

		protected override void OnInitialized() {
			NavigationManager.LocationChanged += OnLocationChanged;
			BuildBreadcrumbs();
		}

		protected override async Task OnAfterRenderAsync(bool firstRender) {
			if (firstRender) {
				await ApplyResponsiveOpenRuleAsync();
				await InvokeAsync(StateHasChanged);
			}
		}

		private async void OnLocationChanged(object? sender, LocationChangedEventArgs e) {
			BuildBreadcrumbs();
			await ApplyResponsiveOpenRuleAsync();
			await InvokeAsync(StateHasChanged);
		}

		// Called by NavBar
		public async Task ToggleDrawerAsync() {
			var width = await GetWindowWidthAsync();
			_isLarge = width >= LargeScreenThreshold;

			// toggle regardless of route; the drawer will ultimately not be visible on the homepage
			_open = !_open;

			// ensure UI updates
			await InvokeAsync(StateHasChanged);
		}

		private async Task ApplyResponsiveOpenRuleAsync() {
			var width = await GetWindowWidthAsync();
			_isLarge = width >= LargeScreenThreshold;

			// On large screens keep the drawer open by default (only if ShowDrawer allows it).
			// On small screens start closed.
			_open = _isLarge && ShowDrawer;
		}

		private async Task<int> GetWindowWidthAsync() {
			const int maxRetries = 5;
			for (int i = 0; i < maxRetries; i++) {
				try {
					return await JSRuntime.InvokeAsync<int>("rise.getWindowWidth");
				} catch (Exception ex) {
					// Log each attempt as a warning so intermittent JS interop failures are visible.
					Logger?.LogWarning(ex, "Attempt {Attempt} to get window width failed via JS interop.", i + 1);

					// brief delay before retrying
					await Task.Delay(100);
				}
			}

			// All retries failed — log an error so this is visible in diagnostics and return a safe default.
			Logger?.LogError("GetWindowWidthAsync: failed to retrieve window width after {MaxRetries} attempts; returning 0 as fallback.", maxRetries);
			return 0;
		}

		private void BuildBreadcrumbs() {
			_breadcrumbs.Clear();

			// use base-relative path; remove query string and leading/trailing slashes
			var relative = NavigationManager.ToBaseRelativePath(NavigationManager.Uri).Split('?').FirstOrDefault() ?? string.Empty;
			relative = relative.Trim('/');

			if (string.IsNullOrEmpty(relative)) {
				// only Home segment -> breadcrumbs contains exactly "Home"
				_breadcrumbs.Add(("Home", "/"));
				return;
			}

			var segments = relative.Split('/', StringSplitOptions.RemoveEmptyEntries);
			var accumulated = string.Empty;

			// add Home link first
			_breadcrumbs.Add(("Home", "/"));

			for (int i = 0; i < segments.Length; i++) {
				accumulated += (i == 0 ? "" : "/") + segments[i];
				// humanize segment if needed (you can integrate a nicer display here)
				var text = Uri.UnescapeDataString(segments[i]).Replace('-', ' ');
				// Capitalize first letter
				if (!string.IsNullOrEmpty(text))
					text = char.ToUpperInvariant(text[0]) + text.Substring(1);

				_breadcrumbs.Add((text, "/" + accumulated));
			}
		}

		public void Dispose() {
			NavigationManager.LocationChanged -= OnLocationChanged;
		}
	}
}