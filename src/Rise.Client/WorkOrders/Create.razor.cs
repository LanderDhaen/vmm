using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Components;
using Rise.Client.Services;
using Rise.Shared.Common;
using Rise.Shared.MaintObjects;
using Rise.Shared.WorkOrders;

namespace Rise.Client.WorkOrders;

public partial class Create {
	[Inject] public IWorkOrderService WorkOrderService { get; set; } = default!;
	[Inject] public IMaintObjService MaintObjService { get; set; } = default!;
	[Inject] public IDialogService DialogService { get; set; } = default!;
	[Inject] public ISnackbar Snackbar { get; set; } = default!;
	[Inject] public NavigationManager NavigationManager { get; set; } = default!;
	[Inject] public BreadcrumbService BreadcrumbService { get; set; } = default!;
	[Inject] public HeaderService HeaderService { get; set; } = default!;

	private readonly WorkOrderRequest.Create.Validator _validator = new();

	private MudForm _form = default!;
	private WorkOrderRequest.Create Model { get; set; } = new();
	private IEnumerable<MaintObjDto.Index>? _maintObjs;

	private MaintObjDto.Index? _selectedMaintObj;

	protected override async Task OnInitializedAsync() {
		QueryRequest.SkipTake req = new() {
			Skip = 0,
			Take = Int32.MaxValue,
			OrderBy = "Name"
		};

		Result<MaintObjResponse.Index> moResult = await MaintObjService.GetIndexAsync(req);
		_maintObjs = moResult.Value.MaintObjects;

		if (Model.MaintObjId is Guid id && _maintObjs is not null)
			_selectedMaintObj = _maintObjs.FirstOrDefault(m => m.Id == id);

		BreadcrumbService.SetBreadcrumbs(
			new BreadcrumbItem("Werkbonnen", "/workorders"),
			new BreadcrumbItem("Voeg toe", "/workorders/create", true)
		);

		HeaderService.Set("Voeg toe", string.Empty);
	}

	private Task<IEnumerable<MaintObjDto.Index>> SearchMaintObjs(string value, CancellationToken ct) {
		if (_maintObjs is null)
			return Task.FromResult(Enumerable.Empty<MaintObjDto.Index>());

		if (string.IsNullOrWhiteSpace(value))
			return Task.FromResult(_maintObjs);

		value = value.Trim();

		var results = _maintObjs.Where(mo =>
			(!string.IsNullOrWhiteSpace(mo.Name) &&
			 mo.Name.Contains(value, StringComparison.OrdinalIgnoreCase)) ||
			(!string.IsNullOrWhiteSpace(mo.SerialNumber) &&
			 mo.SerialNumber.Contains(value, StringComparison.OrdinalIgnoreCase))
		);

		return Task.FromResult(results);
	}

	private string MaintObjToString(MaintObjDto.Index? mo)
		=> mo is null ? string.Empty : $"{mo.Name} ({mo.SerialNumber})";

	private Task OnMaintObjChanged() {
		Model.MaintObjId = _selectedMaintObj?.Id;
		return Task.CompletedTask;
	}

	private async Task OpenQrScanner() {
		var options = new DialogOptions {
			CloseButton = true,
			MaxWidth = MaxWidth.Small,
			FullWidth = true
		};

		var dialog = await DialogService.ShowAsync<QrScanDialog>("Scan QR code", options);
		var result = await dialog.Result;

		if (result!.Canceled || result.Data is not string decoded)
			return;

		var serial = decoded.Trim();

		if (string.IsNullOrWhiteSpace(serial)) {
			Snackbar.Add("Onleesbare code: geen serienummer gevonden.", Severity.Error);
			return;
		}

		var match = _maintObjs?.FirstOrDefault(mo =>
			string.Equals(mo.SerialNumber, serial, StringComparison.OrdinalIgnoreCase));

		if (match is null) {
			var existsResult = await MaintObjService.GetBySerialAsync(serial);
			if (existsResult.IsSuccess) {
				match = _maintObjs?.FirstOrDefault(mo => mo.Id == existsResult.Value.Id);
			}
		}

		if (match is null) {
			Snackbar.Add($"Geen onderhoudsobject gevonden voor code '{serial}'.", Severity.Warning);
			return;
		}

		_selectedMaintObj = match;
		Model.MaintObjId = match.Id;

		Snackbar.Add("Onderhoudsobject geselecteerd via scan.", Severity.Success);
		StateHasChanged();
	}

	private async Task CreateWorkOrderAsync() {
		await _form.Validate();

		if (!_form.IsValid)
			return;

		Result<WorkOrderResponse.Create> result =
			await WorkOrderService.CreateAsync(Model);

		if (result.IsSuccess) {
			Snackbar.Add("Werkbon succesvol aangemaakt!", Severity.Success);
			NavigationManager.NavigateTo("/workorders");
		} else {
			Snackbar.Add(string.Join("\n", result.Errors), Severity.Error);
		}
	}
}