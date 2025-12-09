using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.Common;
using Rise.Shared.Projects;
using Rise.Shared.Stations;

namespace Rise.Client.Components.Projects;

public partial class UpdateStationListDialog {
	// Radio button
	private string _selectedOption = "Bestaand";

	private IEnumerable<MeasurementStationDto.Index>? _allStations = new List<MeasurementStationDto.Index>();
	private Result? _result;
	private CancellationTokenSource _cancellationTokenSource = new();
	private string _searchTerm = string.Empty;

	// Form
	private readonly MeasurementStationRequest.Create.Validator _validator = new();
	private Result<MeasurementStationResponse.Create>? _createResult;
	private MudForm _form;

	private MeasurementStationRequest.Create CreateModel { get; } = new() {
		IsNewLocation = false
	};


	private ProjectRequest.UpdateMeasurementStations Model { get; } = new();

	[CascadingParameter]
	private IMudDialogInstance MudDialog { get; set; }
	[Parameter] public Guid ProjectId { get; set; }
	[Parameter] public IEnumerable<MeasurementStationDto.Index>? Stations { get; set; }
	[Inject] public required IProjectService ProjectService { get; set; }
	[Inject] public required IMeasurementStationService StationService { get; set; }
	[Inject] private ISnackbar Snackbar { get; set; } = default!;

	protected override async Task OnInitializedAsync() {
		Model.ProjectId = ProjectId;
		Model.MeasurementStationIds = Stations?.Select(s => s.Id).ToList() ?? [];
		await GetAllStationsAsync();
	}

	private void CloseOverlay() {
		MudDialog.Close(DialogResult.Ok(true));
	}

	private void Cancel() => MudDialog.Cancel();

	private async Task UpdateSearch(string searchTerm) {
		_searchTerm = searchTerm;
		await GetAllStationsAsync();
	}

	private void OnCheckboxChanged(Guid stationId, bool isSelected) {
		if (isSelected)
			Model.MeasurementStationIds.Add(stationId);
		else
			Model.MeasurementStationIds.Remove(stationId);
	}

	private async Task GetAllStationsAsync() {
		await _cancellationTokenSource.CancelAsync();
		_cancellationTokenSource.Dispose();
		_cancellationTokenSource = new CancellationTokenSource();

		QueryRequest.SkipTake request = new() {
			Skip = 0,
			Take = Int32.MaxValue,
			SearchTerm = _searchTerm
		};

		Result<MeasurementStationResponse.Index> allStations =
			await StationService.GetIndexAsync(request, _cancellationTokenSource.Token);

		// --- Load in all stations for select ---
		if (allStations is { IsSuccess: true, Value: not null }) {
			_allStations = allStations.Value.Stations;
		} else {
			string message = allStations.Errors != null
				? string.Join(Environment.NewLine, allStations.Errors)
				: "Kon meetstations niet laden.";
			Snackbar.Add(message, Severity.Warning);
		}
	}

	private async Task Save() {
		if (_selectedOption == "Bestaand") {
			await UpdateMeasurementStations();
		} else {
			await CreateProjectAsync();
		}
	}

	private async Task UpdateMeasurementStations() {
		bool selectionChanged = !Model.MeasurementStationIds.SequenceEqual(
			Stations?.Select(s => s.Id) ?? []);

		if (!selectionChanged) {
			Snackbar.Add("Je moet minstens 1 meetstation selecteren.", Severity.Error);
			return;
		}

		_result = await ProjectService.UpdateMeasurementStationsAsync(Model);
		if (_result.IsSuccess) {
			Snackbar.Add("Meetstations succesvol bijgewerkt!", Severity.Success);
			Result<ProjectResponse.MeasurementStations> stationsResult =
				await ProjectService.GetStationsAsync(ProjectId);
			if (stationsResult is { IsSuccess: true, Value: not null }) {
				Stations = stationsResult.Value.MeasurementStationsFromProject;
			}

			CloseOverlay();
		} else {
			Snackbar.Add(string.Join("\n", _result.Errors), Severity.Error);
		}
	}

	private async Task CreateProjectAsync() {
		await _form.Validate();

		if (_form.IsValid) {
			CreateModel.ProjectId = ProjectId;
			_createResult = await StationService.CreateAsync(CreateModel);
			if (_createResult.IsSuccess) {
				Snackbar.Add("Meetstation succesvol aangemaakt!", Severity.Success);

				CloseOverlay();
			} else {
				Snackbar.Add(string.Join("\n", _createResult.Errors), Severity.Error);
			}
		}
	}
}