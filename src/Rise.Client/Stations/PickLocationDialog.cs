using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using MudBlazor;

namespace Rise.Client.Stations;

public partial class PickLocationDialog {
	private CoordinateResult? _picked;
	[CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = default!;

	[Parameter] public double InitialX { get; set; }
	[Parameter] public double InitialY { get; set; }

	protected override async Task OnAfterRenderAsync(bool firstRender) {
		if (!firstRender) {
			return;
		}

		double x = InitialX != 0 ? InitialX : 4.0; // longitude
		double y = InitialY != 0 ? InitialY : 51.0; // latitude

		await JS.InvokeVoidAsync(
			"pickMap.init",
			"pick-map",
			y, x,
			DotNetObjectReference.Create(this)
		);
	}

	[JSInvokable]
	public void OnMapClicked(double lat, double lng) {
		_picked = new CoordinateResult(lng, lat);
		StateHasChanged();
	}

	private void Confirm() {
		MudDialog.Close(DialogResult.Ok(_picked));
	}

	private void Cancel() {
		MudDialog.Cancel();
	}
}