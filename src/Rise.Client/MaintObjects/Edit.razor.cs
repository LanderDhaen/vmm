using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Components;
using Rise.Client.Services;
using Rise.Shared.Common;
using Rise.Shared.Locations;
using Rise.Shared.MaintObjects;

namespace Rise.Client.MaintObjects;

public partial class Edit {
	private readonly MaintObjRequest.Edit.Validator _validator = new();
	private IEnumerable<LocationDto.Index>? _locations;
	private Result? _result;
	private MudForm form;

	[Parameter] public Guid MaintObjId { get; set; }

	[Inject] private ISnackbar Snackbar { get; set; }
	[Inject] public required IMaintObjService MaintObjService { get; set; }
	[Inject] public required ILocationService LocationService { get; set; }
	[Inject] public required NavigationManager NavigationManager { get; set; }
	[Inject] public required HeaderService HeaderService { get; set; }
	[Inject] public required BreadcrumbService BreadcrumbService { get; set; }
	[Inject] public required IDialogService DialogService { get; set; }

	private MaintObjRequest.Edit Model { get; set; }

	protected override async Task OnInitializedAsync() {
		Result<MaintObjResponse.Detail> maintObj = await MaintObjService.GetDetailAsync(MaintObjId);
		Result<LocationResponse.Index> result = await LocationService.GetIndexAsync(new QueryRequest.SkipTake());
		_locations = result.Value.Locations;

		if (maintObj.IsSuccess) {
			Model = new MaintObjRequest.Edit {
				MaintObjId = maintObj.Value.MaintObj.Id,
				Name = maintObj.Value.MaintObj.Name,
				SerialNumber = maintObj.Value.MaintObj.SerialNumber,
				MaintObjType = maintObj.Value.MaintObj.MaintObjType,
				Brand = maintObj.Value.MaintObj.Brand,
				InstalledAt = maintObj.Value.MaintObj.InstalledAt,
				RemovedAt = maintObj.Value.MaintObj.RemovedAt,
				LocationId = maintObj.Value.MaintObj.LocationId
			};
		}

		BreadcrumbService.SetBreadcrumbs(
			new BreadcrumbItem("Onderhoudsobjecten", "/maintobjects"),
			new BreadcrumbItem($"{maintObj.Value.MaintObj.Name}", $"/maintobjects/{MaintObjId}"),
			new BreadcrumbItem("Bewerk", $"/maintobjects/edit/{MaintObjId}", true)
		);

		// set header
		HeaderService.Set(
			"Bewerk",
			Icons.Material.Filled.ArrowBack,
			() => {
				NavigationManager.NavigateTo("/maintobjects/");
				return Task.CompletedTask;
			}
		);
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
		if (existsResult.IsSuccess && existsResult.Value.Id != Model.MaintObjId) {
			Snackbar.Add("Dit serienummer hoort al bij een ander toestel.", Severity.Warning);
		}

		Model.SerialNumber = serial;
		Snackbar.Add("Serienummer aangepast via scan.", Severity.Success);
		StateHasChanged();
	}

	private async Task EditMaintObjAsync() {
		await form.Validate();

		if (form.IsValid) {
			_result = await MaintObjService.EditAsync(Model, CancellationToken.None);
			if (_result.IsSuccess) {
				Snackbar.Add("Onderhoudsobject succesvol aangepast!", Severity.Success);
				await Task.Delay(2000);
				NavigationManager.NavigateTo("/maintobjects");
			} else {
				Snackbar.Add(string.Join("\n", _result.Errors), Severity.Error);
			}
		}
	}
}