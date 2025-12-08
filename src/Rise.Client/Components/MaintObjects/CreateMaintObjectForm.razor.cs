using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.Common;
using Rise.Shared.Locations;
using Rise.Shared.MaintObjects;

namespace Rise.Client.Components.MaintObjects;

public partial class CreateMaintObjectForm {
	private IEnumerable<LocationDto.Index>? _locations;

	[Parameter] public required MaintObjRequest.Create Model { get; set; } = new();
	
	[Inject] private ISnackbar Snackbar { get; set; }
	[Inject] public required IMaintObjService MaintObjService { get; set; }
	[Inject] public required ILocationService LocationService { get; set; }
	[Inject] public required NavigationManager NavigationManager { get; set; }
	[Inject] public required IDialogService DialogService { get; set; }

	protected override async Task OnInitializedAsync() {
		QueryRequest.SkipTake request = new();
		request.Filters["isStockLocation"] = true;

		Result<LocationResponse.Index> result = await LocationService.GetIndexAsync(request);
		_locations = result.Value.Locations;
	}

	private async Task OpenQrScanner() {
		var options = new DialogOptions {
			CloseButton = true,
			MaxWidth = MaxWidth.Small,
			FullWidth = true
		};

		var dialog = DialogService.Show<QrScanDialog>("Scan QR code", options);
		var result = await dialog.Result;

		if (result.Canceled || result.Data is not string decoded)
			return;

		var serial = decoded.Trim();

		if (string.IsNullOrWhiteSpace(serial)) {
			Snackbar.Add("Onleesbare code: geen serienummer gevonden.", Severity.Error);
			return;
		}

		var existsResult = await MaintObjService.GetBySerialAsync(serial);

		if (existsResult.IsSuccess) {
			NavigationManager.NavigateTo($"/maintObjects/{existsResult.Value.Id}");
			return;
		}

		Model.SerialNumber = serial;
		Snackbar.Add("Serienummer ingevuld via scan.", Severity.Success);
		StateHasChanged();
	}
}