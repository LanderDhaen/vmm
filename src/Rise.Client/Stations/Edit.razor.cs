using FluentValidation.Results;

using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Services;
using Rise.Shared.Common;
using Rise.Shared.Locations;
using Rise.Shared.Projects;
using Rise.Shared.Stations;

using Severity = MudBlazor.Severity;

namespace Rise.Client.Stations;

public partial class Edit {
	private readonly MeasurementStationRequest.Edit.Validator _validator = new();
	private MudForm _form = null!;

	private IEnumerable<LocationDto.Index> _locations = [];

	private MeasurementStationRequest.Edit? _model;
	private IEnumerable<ProjectDto.Index> _projects = [];
	[Parameter] public Guid StationId { get; set; }

	protected Func<object, string, IEnumerable<string>> FieldValidator =>
		(model, propertyName) => {
			if (model is not MeasurementStationRequest.Edit m) {
				return [];
			}

			ValidationResult? result = _validator.Validate(m);
			return result.Errors
				.Where(e => e.PropertyName == propertyName)
				.Select(e => e.ErrorMessage);
		};

	[Inject] private ISnackbar Snackbar { get; set; } = null!;
	[Inject] private IMeasurementStationService StationService { get; set; } = null!;
	[Inject] private ILocationService LocationService { get; set; } = null!;
	[Inject] private IProjectService ProjectService { get; set; } = null!;
	[Inject] private NavigationManager Nav { get; set; } = null!;
	[Inject] public required BreadcrumbService BreadcrumbService { get; set; }
	[Inject] public required HeaderService HeaderService { get; set; }

	protected override async Task OnInitializedAsync() {
		Result<LocationResponse.Index> locsResult = await LocationService.GetIndexAsync(new QueryRequest.SkipTake());
		_locations = locsResult.Value.Locations;

		Result<ProjectResponse.Index> projResult =
			await ProjectService.GetIndexAsync(new QueryRequest.SkipTake { Skip = 0, Take = 1000 });
		_projects = projResult.Value.Projects;

		Result<MeasurementStationResponse.Detail> detail = await StationService.GetDetailAsync(StationId);

		Result<List<ProjectDto.Index>> linked = await StationService.GetProjectsAsync(StationId);
		Guid preselectedProjectId = linked.IsSuccess && linked.Value.Count != 0
			? linked.Value.First().Id
			: _projects.FirstOrDefault()?.Id ?? Guid.Empty;

		Guid locationId = _locations
			.FirstOrDefault(l =>
				string.Equals(l.Name, detail.Value.Station.Location, StringComparison.OrdinalIgnoreCase) &&
				string.Equals(l.Address.City, detail.Value.Station.City, StringComparison.OrdinalIgnoreCase))
			?.Id ?? _locations.FirstOrDefault()?.Id ?? Guid.Empty;

		_model = new MeasurementStationRequest.Edit {
			StationId = StationId,
			Code = detail.Value.Station.Code,
			InstalledAt = detail.Value.Station.InstalledAt,
			RemovedAt = detail.Value.Station.RemovedAt,
			LocationId = locationId,
			ProjectId = preselectedProjectId
		};


		BreadcrumbService.SetBreadcrumbs(
			new BreadcrumbItem("Meetstations", "/stations"),
			new BreadcrumbItem($"{detail.Value.Station.Code}", $"/stations/{StationId}"),
			new BreadcrumbItem("Bewerk", $"/stations/edit/{StationId}", true)
		);

		// set header
		HeaderService.Set(
			"Bewerk",
			Icons.Material.Filled.ArrowBack,
			() => {
				Navigation.NavigateTo("/stations/");
				return Task.CompletedTask;
			}
		);
	}

	private async Task EditStationAsync() {
		await _form.Validate();

		if (_form.IsValid && _model is not null) {
			Result result = await StationService.EditAsync(_model, CancellationToken.None);
			if (result.IsSuccess) {
				Snackbar.Add("Meetstation succesvol aangepast!", Severity.Success);
				await Task.Delay(1200);
				Nav.NavigateTo("/stations");
			} else {
				Snackbar.Add(string.Join("\n", result.Errors), Severity.Error);
			}
		}
	}
}