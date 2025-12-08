using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.MaintObjects;

namespace Rise.Client.Components.MaintObjects;

public partial class EditFormDialog {
	private readonly MaintObjRequest.Edit.Validator _validator = new();
	private MudForm _form = null!;
	private Result? _result;

	[Parameter] public Guid MaintObjId { get; set; }
	[Parameter] public MaintObjDto.Detail MaintObj { get; set; } = null!;
	[CascadingParameter] private IMudDialogInstance MudDialog { get; set; } = null!;
	[Inject] private ISnackbar Snackbar { get; set; } = null!;
	[Inject] public required IMaintObjService MaintObjService { get; set; } = null!;
	[Inject] public required IDialogService DialogService { get; set; } = null!;
	private MaintObjRequest.Edit Model { get; set; } = null!;

	protected override Task OnInitializedAsync() {
		if (MaintObj is not null) {
			Model = new MaintObjRequest.Edit {
				MaintObjId = MaintObj.Id,
				Name = MaintObj.Name,
				SerialNumber = MaintObj.SerialNumber,
				Brand = MaintObj.Brand,
				InstalledAt = MaintObj.InstalledAt,
				RemovedAt = MaintObj.RemovedAt
			};
		}

		return Task.CompletedTask;
	}

	private void Cancel() {
		MudDialog.Cancel();
	}

	private async Task OpenQrScanner() {
		DialogOptions options = new() {
			CloseButton = true,
			MaxWidth = MaxWidth.Small,
			FullWidth = true
		};

		IDialogReference dialogRef = await DialogService.ShowAsync<QrScanDialog>("Scan QR code", options);

		DialogResult? result = await dialogRef.Result;
		if (result is null || result.Canceled || result.Data is not string decoded) {
			return;
		}

		string serial = decoded.Trim();

		if (string.IsNullOrWhiteSpace(serial)) {
			Snackbar.Add("Onleesbare code: geen serienummer gevonden.", Severity.Error);
			return;
		}

		Result<MaintObjResponse.BySerial> existsResult = await MaintObjService.GetBySerialAsync(serial);
		if (existsResult.IsSuccess && existsResult.Value.Id != Model.MaintObjId) {
			Snackbar.Add("Dit serienummer hoort al bij een ander toestel.", Severity.Warning);
		}

		Model.SerialNumber = serial;
		Snackbar.Add("Serienummer aangepast via scan.", Severity.Success);
		StateHasChanged();
	}

	private async Task EditMaintObjAsync() {
		await _form.Validate();

		if (_form.IsValid) {
			_result = await MaintObjService.EditAsync(Model, CancellationToken.None);
			if (_result.IsSuccess) {
				Snackbar.Add("Onderhoudsobject succesvol aangepast!", Severity.Success);
				MudDialog.Close(DialogResult.Ok(true));
			} else {
				Snackbar.Add(string.Join("\n", _result.Errors), Severity.Error);
			}
		}
	}
}