using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Components;
using Rise.Client.Services;
using Rise.Shared.Common;
using Rise.Shared.Locations;
using Rise.Shared.MaintObjects;

namespace Rise.Client.MaintObjects;

public partial class Create {
	private readonly MaintObjRequest.Create.Validator _validator = new();
	private IEnumerable<LocationDto.Index>? _locations;
	private Result<MaintObjResponse.Create>? _result;
	private MudForm _form;

	[Inject] private ISnackbar Snackbar { get; set; }
	[Inject] public required IMaintObjService MaintObjService { get; set; }
	[Inject] public required ILocationService LocationService { get; set; }
	[Inject] public required NavigationManager NavigationManager { get; set; }
	[Inject] public required HeaderService HeaderService { get; set; }
	[Inject] public required BreadcrumbService BreadcrumbService { get; set; }
	[Inject] public required IDialogService DialogService { get; set; }

	private MaintObjRequest.Create Model { get; } = new();

	protected override async Task OnInitializedAsync() {
		QueryRequest.SkipTake request = new();
		request.Filters["isStockLocation"] = true;

		// var query = new QueryRequest.SkipTake();
		// query.Filters.Add("IsStockLocation", "true");

		Result<LocationResponse.Index> result = await LocationService.GetIndexAsync(request);
		_locations = result.Value.Locations;

		BreadcrumbService.SetBreadcrumbs(
			new BreadcrumbItem("Onderhoudsobjecten", "/maintobjects"),
			new BreadcrumbItem("Voeg toe", "/create", true)
		);

		HeaderService.Set(
			"Voeg toe",
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

		var dialog = await DialogService.ShowAsync<QrScanDialog>("Scan QR code", options);
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


	private async Task CreateMaintObjAsync() {
		await _form.Validate();

		if (_form.IsValid) {
			_result = await MaintObjService.CreateAsync(Model);
			if (_result.IsSuccess) {
				Snackbar.Add("Onderhoudsobject succesvol aangemaakt!", Severity.Success);
				await Task.Delay(2000);
				NavigationManager.NavigateTo("/maintobjects");
			} else {
				Snackbar.Add(string.Join("\n", _result.Errors), Severity.Error);
			}
		}
	}
}