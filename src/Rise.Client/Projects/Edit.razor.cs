using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Services;
using Rise.Shared.Common;
using Rise.Shared.Networks;
using Rise.Shared.Projects;

namespace Rise.Client.Projects;

public partial class Edit {
	private readonly ProjectRequest.Edit.Validator _validator = new();
	private IEnumerable<MeasurementNetworkDto.Index>? _networks;
	private Result? _result;
	private MudForm form;
	[Parameter] public Guid ProjectId { get; set; }
	[Inject] private ISnackbar Snackbar { get; set; }
	[Inject] public required IProjectService ProjectService { get; set; }
	[Inject] public required INetworkService NetworkService { get; set; }
	[Inject] public required NavigationManager NavigationManager { get; set; }
	private ProjectRequest.Edit Model { get; set; }
	[Inject] public required BreadcrumbService BreadcrumbService { get; set; }
	[Inject] public required HeaderService HeaderService { get; set; }

	protected override async Task OnInitializedAsync() {
		Result<NetworkResponse.Index> result = await NetworkService.GetIndexAsync(new QueryRequest.SkipTake());
		_networks = result.Value.measurementsNetworks;

		Result<ProjectResponse.Detail> project = await ProjectService.GetDetailAsync(ProjectId);

		if (project.IsSuccess) {
			Model = new ProjectRequest.Edit {
				ProjectId = project.Value.Project.Id,
				Name = project.Value.Project.Name,
				Code = project.Value.Project.Code,
				StartDate = new DateTime(
					project.Value.Project.StartDate.Year,
					project.Value.Project.StartDate.Month,
					project.Value.Project.StartDate.Day),
				EndDate = project.Value.Project.EndDate is { } endDate
					? new DateTime(endDate.Year, endDate.Month, endDate.Day)
					: null,
				MeasurementNetworkId = project.Value.Project.MeasurementNetwork.Id
			};
		}

		BreadcrumbService.SetBreadcrumbs(
			new BreadcrumbItem("Projecten", "/projects"),
			new BreadcrumbItem($"{project.Value.Project.Code} - {project.Value.Project.Name}",
				$"/projects/{ProjectId}"),
			new BreadcrumbItem("Bewerk", $"/projects/edit/{ProjectId}", true)
		);

		// set header
		HeaderService.Set(
			"Bewerk",
			Icons.Material.Filled.ArrowBack,
			() => {
				Navigation.NavigateTo("/projects/");
				return Task.CompletedTask;
			}
		);
	}

	private async Task EditProjectAsync() {
		await form.Validate();

		if (form.IsValid) {
			_result = await ProjectService.EditAsync(Model, CancellationToken.None);
			if (_result.IsSuccess) {
				Snackbar.Add("Project succesvol aangepast!", Severity.Success);
				await Task.Delay(2000);
				NavigationManager.NavigateTo("/projects");
			} else {
				Snackbar.Add(string.Join("\n", _result.Errors), Severity.Error);
			}
		}
	}
}