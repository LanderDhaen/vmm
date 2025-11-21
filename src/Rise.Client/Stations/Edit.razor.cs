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
	private readonly MeasurementStationRequest.Edit.Validator validator = new();

	private IEnumerable<LocationDto.Index> _locations = Array.Empty<LocationDto.Index>();
	private IEnumerable<ProjectDto.Index> _projects = Array.Empty<ProjectDto.Index>();
	private MudForm form = default!;

	private MeasurementStationRequest.Edit? Model;
	[Parameter] public Guid StationId { get; set; }

	protected Func<object, string, IEnumerable<string>> FieldValidator =>
		(model, propertyName) => {
			if (model is not MeasurementStationRequest.Edit m) {
				return Enumerable.Empty<string>();
			}

			ValidationResult? result = validator.Validate(m);
			return result.Errors
				.Where(e => e.PropertyName == propertyName)
				.Select(e => e.ErrorMessage);
		};

	[Inject] private ISnackbar Snackbar { get; set; } = default!;
	[Inject] private IMeasurementStationService StationService { get; set; } = default!;
	[Inject] private ILocationService LocationService { get; set; } = default!;
	[Inject] private IProjectService ProjectService { get; set; } = default!;
	[Inject] private NavigationManager Nav { get; set; } = default!;
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
		Guid preselectedProjectId = linked.IsSuccess && linked.Value.Any()
			? linked.Value.First().Id
			: _projects.FirstOrDefault()?.Id ?? Guid.Empty;

		Guid locationId = _locations
			.FirstOrDefault(l =>
				string.Equals(l.Name, detail.Value.Station.Location, StringComparison.OrdinalIgnoreCase) &&
				string.Equals(l.Address.City, detail.Value.Station.City, StringComparison.OrdinalIgnoreCase))
			?.Id ?? _locations.FirstOrDefault()?.Id ?? Guid.Empty;

		Model = new MeasurementStationRequest.Edit {
			StationId = StationId,
			Code = detail.Value.Station.Code,
			InstalledAt = detail.Value.Station.InstalledAt,
			RemovedAt = detail.Value.Station.RemovedAt,
			LocationId = locationId,
			ProjectId = preselectedProjectId
		};


		BreadcrumbService.SetBreadcrumbs(
			new BreadcrumbItem("Home", "/"),
			new BreadcrumbItem("Meetstations", "/stations"),
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
		await form.Validate();

		if (form.IsValid && Model is not null) {
			Result result = await StationService.EditAsync(Model, CancellationToken.None);
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