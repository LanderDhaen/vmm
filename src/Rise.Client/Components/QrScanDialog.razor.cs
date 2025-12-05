using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

using MudBlazor;

namespace Rise.Client.Components;

public class QrScanDialogBase : ComponentBase, IAsyncDisposable {
	[Inject] protected IJSRuntime JS { get; set; } = default!;

	[CascadingParameter]
	protected IMudDialogInstance MudDialog { get; set; } = default!;


	protected string? Error { get; set; }

	private DotNetObjectReference<QrScanDialogBase>? _dotNetRef;

	protected override async Task OnAfterRenderAsync(bool firstRender) {
		if (!firstRender) return;

		_dotNetRef = DotNetObjectReference.Create(this);
		await JS.InvokeVoidAsync("qrScanner.start", "qr-reader", _dotNetRef);
	}

	[JSInvokable]
	public void OnQrScanned(string decodedText) {
		MudDialog.Close(DialogResult.Ok(decodedText));
	}

	[JSInvokable]
	public void OnQrError(string message) {
		Error = message;
		InvokeAsync(StateHasChanged);
	}

	protected async Task Cancel() {
		await JS.InvokeVoidAsync("qrScanner.stop");
		MudDialog.Cancel();
	}

	public async ValueTask DisposeAsync() {
		try {
			await JS.InvokeVoidAsync("qrScanner.stop");
		} catch {

		}

		_dotNetRef?.Dispose();
	}
}