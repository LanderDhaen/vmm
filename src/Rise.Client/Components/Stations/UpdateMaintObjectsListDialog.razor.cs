using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.Common;
using Rise.Shared.MaintObjects;
using Rise.Shared.Stations;

namespace Rise.Client.Components.Stations;

public partial class UpdateMaintObjectsListDialog {
	// Radio button
	private string _selectedOption = "Bestaand";

	private IEnumerable<MaintObjDto.Index>? _allMaintObjects = new List<MaintObjDto.Index>();
	private Result? _result;
	private CancellationTokenSource _cancellationTokenSource = new();
	private string _searchTerm = string.Empty;

	// Form
	private readonly MaintObjRequest.Create.Validator _validator = new();
	private Result<MaintObjResponse.Create>? _createResult;
	private readonly MudForm _form;

	private MaintObjRequest.Create CreateModel { get; } = new();

	private MeasurementStationRequest.UpdateMaintObjects Model { get; } = new();

	[CascadingParameter]
	private IMudDialogInstance MudDialog { get; set; }
	[Parameter] public Guid StationId { get; set; }
	[Parameter] public IEnumerable<MaintObjDto.Index>? MaintObjects { get; set; }
	[Inject] public required IMaintObjService MaintObjService { get; set; }
	[Inject] public required IMeasurementStationService StationService { get; set; }
	[Inject] private ISnackbar Snackbar { get; set; } = default!;

	protected override async Task OnInitializedAsync() {
		Model.MeasurementStationId = StationId;
		Model.MaintObjIds = MaintObjects?.Select(s => s.Id).ToList() ?? [];
		await GetAllMaintObjectsAsync();
	}

	private void CloseOverlay() {
		MudDialog.Close(DialogResult.Ok(true));
	}

	private void Cancel() => MudDialog.Cancel();

	private async Task UpdateSearch(string searchTerm) {
		_searchTerm = searchTerm;
		await GetAllMaintObjectsAsync();
	}

	private void OnCheckboxChanged(Guid stationId, bool isSelected) {
		if (isSelected)
			Model.MaintObjIds.Add(stationId);
		else
			Model.MaintObjIds.Remove(stationId);
	}

	private async Task GetAllMaintObjectsAsync() {
		await _cancellationTokenSource.CancelAsync();
		_cancellationTokenSource.Dispose();
		_cancellationTokenSource = new CancellationTokenSource();

		QueryRequest.SkipTake request = new() {
			SearchTerm = _searchTerm
		};

		request.Filters["Status"] = "Active";
		request.Filters["StationId"] = StationId;

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

	private async Task Save() {
		if (_selectedOption == "Bestaand") {
			await UpdateMaintObjects();
		} else {
			await CreateMaintObjectAsync();
		}
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

	private async Task CreateMaintObjectAsync() {
		await _form.Validate();

		if (_form.IsValid) {
			//CreateModel.MeasurementStationId = StationId;
			_createResult = await MaintObjService.CreateAsync(CreateModel);
			if (_createResult.IsSuccess) {
				Snackbar.Add("Onderhoudsobject succesvol aangemaakt!", Severity.Success);

				CloseOverlay();
			} else {
				Snackbar.Add(string.Join("\n", _createResult.Errors), Severity.Error);
			}
		}
	}
}