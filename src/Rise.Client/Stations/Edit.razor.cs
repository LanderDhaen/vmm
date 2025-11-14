using System.Linq;

using FluentValidation;

using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.Common;
using Rise.Shared.Locations;
using Rise.Shared.Projects;
using Rise.Shared.Stations;

using Severity = MudBlazor.Severity;

namespace Rise.Client.Stations;

public partial class Edit {
	[Parameter] public Guid StationId { get; set; }

	private MeasurementStationRequest.Edit? Model;
	private readonly MeasurementStationRequest.Edit.Validator validator = new();
	private MudForm form = default!;

	private IEnumerable<LocationDto.Index> _locations = Array.Empty<LocationDto.Index>();
	private IEnumerable<ProjectDto.Index> _projects = Array.Empty<ProjectDto.Index>();

	private readonly string[] _statuses = ["Actief", "Inactief"];

	protected Func<object, string, IEnumerable<string>> FieldValidator =>
		(model, propertyName) => {
			if (model is not MeasurementStationRequest.Edit m) return Enumerable.Empty<string>();
			var result = validator.Validate(m);
			return result.Errors
						 .Where(e => e.PropertyName == propertyName)
						 .Select(e => e.ErrorMessage);
		};

	[Inject] private ISnackbar Snackbar { get; set; } = default!;
	[Inject] private IMeasurementStationService StationService { get; set; } = default!;
	[Inject] private ILocationService LocationService { get; set; } = default!;
	[Inject] private IProjectService ProjectService { get; set; } = default!;
	[Inject] private NavigationManager Nav { get; set; } = default!;

	protected override async Task OnInitializedAsync() {
		var locsResult = await LocationService.GetIndexAsync(new QueryRequest.SkipTake());
		_locations = locsResult.Value.Locations;

		var projResult = await ProjectService.GetIndexAsync(new QueryRequest.SkipTake { Skip = 0, Take = 1000 });
		_projects = projResult.Value.Projects;

		var detail = await StationService.GetDetailAsync(StationId);

		var linked = await StationService.GetProjectsAsync(StationId);
		var preselectedProjectId = linked.IsSuccess && linked.Value.Any()
			? linked.Value.First().Id
			: _projects.FirstOrDefault()?.Id ?? Guid.Empty;

		var locationId = _locations
			.FirstOrDefault(l =>
				string.Equals(l.Name, detail.Value.Station.Location, StringComparison.OrdinalIgnoreCase) &&
				string.Equals(l.Address.City, detail.Value.Station.City, StringComparison.OrdinalIgnoreCase))
			?.Id ?? _locations.FirstOrDefault()?.Id ?? Guid.Empty;

		var status = detail.Value.Station.Status;
		if (!_statuses.Contains(status, StringComparer.OrdinalIgnoreCase)) {
			status = status.Equals("Active", StringComparison.OrdinalIgnoreCase) ? "Actief"
				   : status.Equals("Inactive", StringComparison.OrdinalIgnoreCase) ? "Inactief"
				   : "Actief";
		}

		Model = new MeasurementStationRequest.Edit {
			StationId = StationId,
			Code = detail.Value.Station.Code,
			Status = status,
			InstalledAt = detail.Value.Station.InstalledAt,
			RemovedAt = detail.Value.Station.RemovedAt,
			LocationId = locationId,
			ProjectId = preselectedProjectId
		};
	}

	private async Task EditStationAsync() {
		await form.Validate();

		if (form.IsValid && Model is not null) {
			var result = await StationService.EditAsync(Model, CancellationToken.None);
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