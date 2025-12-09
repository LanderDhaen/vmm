using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.Common;
using Rise.Shared.MaintObjects;
using Rise.Shared.Stations;

namespace Rise.Client.Components.Stations;

public partial class UpdateMaintObjectsListDialog {
	private IEnumerable<MaintObjDto.Index>? _allMaintObjects = new List<MaintObjDto.Index>();
	private CancellationTokenSource _cancellationTokenSource = new();
	private Result? _result;
	private string _searchTerm = string.Empty;

	private MeasurementStationRequest.UpdateMaintObjects Model { get; } = new();

	[CascadingParameter] private IMudDialogInstance MudDialog { get; set; }

	[Parameter] public Guid StationId { get; set; }
	[Parameter] public IEnumerable<MaintObjDto.Index>? MaintObjects { get; set; }
	[Inject] public required IMaintObjService MaintObjService { get; set; }
	[Inject] public required IMeasurementStationService StationService { get; set; }
	[Inject] private ISnackbar Snackbar { get; set; } = default!;
	[Inject] public required IDialogService DialogService { get; set; }

	protected override async Task OnInitializedAsync() {
		Model.MeasurementStationId = StationId;
		Model.MaintObjIds = MaintObjects?.Select(s => s.Id).ToList() ?? [];
		await GetAllMaintObjectsAsync();
	}

	private void CloseOverlay() {
		MudDialog.Close(DialogResult.Ok(true));
	}

	private void Cancel() {
		MudDialog.Cancel();
	}

	private async Task UpdateSearch(string searchTerm) {
		_searchTerm = searchTerm;
		await GetAllMaintObjectsAsync();
	}

	private void OnCheckboxChanged(Guid stationId, bool isSelected) {
		if (isSelected) {
			Model.MaintObjIds.Add(stationId);
		} else {
			Model.MaintObjIds.Remove(stationId);
		}
	}

	private async Task GetAllMaintObjectsAsync() {
		await _cancellationTokenSource.CancelAsync();
		_cancellationTokenSource.Dispose();
		_cancellationTokenSource = new CancellationTokenSource();

		QueryRequest.SkipTake request = new() {
			Skip = 0,
			Take = Int32.MaxValue, // Temporary fix due to time constraints
			SearchTerm = _searchTerm,
			Filters = {
				["Status"] = MaintObjStatus.Inactive,
			}
		};

		Result<MaintObjResponse.Index> allMaintObjects =
			await MaintObjService.GetIndexAsync(request, _cancellationTokenSource.Token);

		// --- Load in all stations for select ---
		if (allMaintObjects is { IsSuccess: true, Value: not null }) {
			_allMaintObjects = allMaintObjects.Value.MaintObjects;
		} else {
			string message = allMaintObjects.Errors != null
				? string.Join(Environment.NewLine, allMaintObjects.Errors)
				: "Kon projecten niet laden.";
			Snackbar.Add(message, Severity.Warning);
		}
	}

	private async Task OpenQrScanner() {
		DialogOptions options = new() {
			CloseButton = true,
			MaxWidth = MaxWidth.Small,
			FullWidth = true
		};

		IDialogReference dialog = DialogService.Show<QrScanDialog>("Scan QR code", options);
		DialogResult? result = await dialog.Result;

		if (result is { Canceled: true } || result?.Data is not string decoded) {
			return;
		}

		string serial = decoded.Trim();

		if (string.IsNullOrWhiteSpace(serial)) {
			Snackbar.Add("Onleesbare code: geen serienummer gevonden.", Severity.Error);
			return;
		}

		MaintObjDto.Index? match = _allMaintObjects?.FirstOrDefault(mo =>
			string.Equals(mo.SerialNumber, serial, StringComparison.OrdinalIgnoreCase));

		if (match is null) {
			Result<MaintObjResponse.BySerial> existsResult = await MaintObjService.GetBySerialAsync(serial);
			if (existsResult.IsSuccess) {
				match = _allMaintObjects?.FirstOrDefault(mo => mo.Id == existsResult.Value.Id);
			}
		}

		if (match is null) {
			Snackbar.Add($"Geen onderhoudsobject gevonden voor code '{serial}'.", Severity.Warning);
			return;
		}

		Model.MaintObjIds.Add(match.Id);

		Snackbar.Add("Onderhoudsobject geselecteerd via scan.", Severity.Success);
		StateHasChanged();
	}


	private async Task UpdateMaintObjects() {
		bool selectionChanged = !Model.MaintObjIds.SequenceEqual(
			MaintObjects?.Select(s => s.Id) ?? []);

		if (!selectionChanged) {
			Snackbar.Add("Je moet minstens 1 onderhoudsobject selecteren.", Severity.Error);
			return;
		}

		_result = await StationService.UpdateMaintObjsAsync(Model);
		if (_result.IsSuccess) {
			Snackbar.Add("Onderhoudsobjecten succesvol bijgewerkt!", Severity.Success);
			Result<MeasurementStationResponse.MaintObjList> maintObjectsResult =
				await StationService.GetMaintObjsAsync(StationId);
			if (maintObjectsResult is { IsSuccess: true, Value: not null }) {
				MaintObjects = maintObjectsResult.Value.MaintObjects;
			}

			CloseOverlay();
		} else {
			Snackbar.Add(string.Join("\n", _result.Errors), Severity.Error);
		}
	}
}