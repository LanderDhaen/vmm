using Microsoft.AspNetCore.Components;

using MudBlazor;
using MudBlazor.Utilities;

using Rise.Client.Services;

namespace Rise.Client.Layout;

public partial class MainLayout : LayoutComponentBase {
	private readonly MudTheme _vmm = new() {
		PaletteLight = new PaletteLight {
			Primary = new MudColor("#126B8D")
		}
	};

	private bool _open;

	[Inject] public BreadcrumbService BreadcrumbService { get; set; } = null!;
	[Inject] public HeaderService HeaderService { get; set; } = null!;

	protected override void OnInitialized() {
		BreadcrumbService.OnChange += BreadcrumbsChanged;
		HeaderService.OnChange += HeaderChanged;
	}

	private void ToggleDrawer() {
		_open = !_open;
	}

	private void BreadcrumbsChanged() {
		InvokeAsync(StateHasChanged);
	}

	private void HeaderChanged() {
		InvokeAsync(StateHasChanged);
	}

	public void Dispose() {
		BreadcrumbService.OnChange -= BreadcrumbsChanged;
		HeaderService.OnChange -= HeaderChanged;
	}
}