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

	protected override void OnInitialized() {
		BreadcrumbService.OnChange += BreadcrumbsChanged;
	}

	private void ToggleDrawer() {
		_open = !_open;
	}

	private void BreadcrumbsChanged() {
		InvokeAsync(StateHasChanged);
	}

	public void Dispose() {
		BreadcrumbService.OnChange -= BreadcrumbsChanged;
	}
}