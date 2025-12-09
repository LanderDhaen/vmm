using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.Common;
using Rise.Shared.Projects;
using Rise.Shared.Stations;

namespace Rise.Client.Components.Stations;

public partial class UpdateProjectListDialog {
	// Radio button
	private string _selectedOption = "Bestaand";

	private IEnumerable<ProjectDto.Index>? _allProjects = new List<ProjectDto.Index>();
	private Result? _result;
	private CancellationTokenSource _cancellationTokenSource = new();
	private string _searchTerm = string.Empty;

	// Form
	private readonly ProjectRequest.Create.Validator _validator = new();
	private Result<ProjectResponse.Create>? _createResult;
	private MudForm _form;

	private ProjectRequest.Create CreateModel { get; } = new();

	private MeasurementStationRequest.UpdateProjects Model { get; } = new();

	[CascadingParameter]
	private IMudDialogInstance MudDialog { get; set; }
	[Parameter] public Guid StationId { get; set; }
	[Parameter] public IEnumerable<ProjectDto.Index>? Projects { get; set; }
	[Inject] public required IProjectService ProjectService { get; set; }
	[Inject] public required IMeasurementStationService StationService { get; set; }
	[Inject] private ISnackbar Snackbar { get; set; } = default!;

	protected override async Task OnInitializedAsync() {
		Model.MeasurementStationId = StationId;
		Model.ProjectIds = Projects?.Select(s => s.Id).ToList() ?? [];
		await GetAllProjectsAsync();
	}

	private void CloseOverlay() {
		MudDialog.Close(DialogResult.Ok(true));
	}

	private void Cancel() => MudDialog.Cancel();

	private async Task UpdateSearch(string searchTerm) {
		_searchTerm = searchTerm;
		await GetAllProjectsAsync();
	}

	private void OnCheckboxChanged(Guid stationId, bool isSelected) {
		if (isSelected)
			Model.ProjectIds.Add(stationId);
		else
			Model.ProjectIds.Remove(stationId);
	}

	private async Task GetAllProjectsAsync() {
		await _cancellationTokenSource.CancelAsync();
		_cancellationTokenSource.Dispose();
		_cancellationTokenSource = new CancellationTokenSource();

		QueryRequest.SkipTake request = new() {
			Skip = 0,
			Take = Int32.MaxValue,
			SearchTerm = _searchTerm
		};

		request.Filters["Status"] = "Active";

		Result<ProjectResponse.Index> allProjects =
			await ProjectService.GetIndexAsync(request, _cancellationTokenSource.Token);

		// --- Load in all stations for select ---
		if (allProjects is { IsSuccess: true, Value: not null }) {
			_allProjects = allProjects.Value.Projects;
		} else {
			string message = allProjects.Errors != null
				? string.Join(Environment.NewLine, allProjects.Errors)
				: "Kon projecten niet laden.";
			Snackbar.Add(message, Severity.Warning);
		}
	}

	private async Task Save() {
		if (_selectedOption == "Bestaand") {
			await UpdateProjects();
		} else {
			await CreateProjectAsync();
		}
	}

	private async Task UpdateProjects() {
		bool selectionChanged = !Model.ProjectIds.SequenceEqual(
			Projects?.Select(s => s.Id) ?? []);

		if (!selectionChanged) {
			Snackbar.Add("Je moet minstens 1 project selecteren.", Severity.Error);
			return;
		}

		_result = await StationService.UpdateProjectsAsync(Model);
		if (_result.IsSuccess) {
			Snackbar.Add("Projecten succesvol bijgewerkt!", Severity.Success);
			Result<MeasurementStationResponse.ProjectList> projectsResult =
				await StationService.GetProjectsAsync(StationId);
			if (projectsResult is { IsSuccess: true, Value: not null }) {
				Projects = projectsResult.Value.Projects;
			}

			CloseOverlay();
		} else {
			Snackbar.Add(string.Join("\n", _result.Errors), Severity.Error);
		}
	}

	private async Task CreateProjectAsync() {
		await _form.Validate();

		if (_form.IsValid) {
			CreateModel.MeasurementStationId = StationId;
			_createResult = await ProjectService.CreateAsync(CreateModel);
			if (_createResult.IsSuccess) {
				Snackbar.Add("Project succesvol aangemaakt!", Severity.Success);

				CloseOverlay();
			} else {
				Snackbar.Add(string.Join("\n", _createResult.Errors), Severity.Error);
			}
		}
	}
}