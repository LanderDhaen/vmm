using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.Common;
using Rise.Shared.Networks;
using Rise.Shared.Projects;

namespace Rise.Client.Projects;

public partial class Edit {
	private Result? _result;
	private IEnumerable<MeasurementNetworkDto.Index>? networks;
	[Parameter] public Guid ProjectId { get; set; }
	[Inject] private ISnackbar Snackbar { get; set; }
	[Inject] public required IProjectService ProjectService { get; set; }
	[Inject] public required INetworkService NetworkService { get; set; }
	[Inject] public required NavigationManager NavigationManager { get; set; }
	private ProjectRequest.Edit Model { get; set; }
	private readonly ProjectRequest.Edit.Validator validator = new();
	MudForm form;

	protected override async Task OnInitializedAsync() {
		Result<NetworkResponse.Index> result = await NetworkService.GetIndexAsync(new QueryRequest.SkipTake());
		networks = result.Value.measurementsNetworks;
		
		Result<ProjectResponse.Detail> project = await ProjectService.GetDetailAsync(ProjectId);

		if (project.IsSuccess) {
			Model = new ProjectRequest.Edit{
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

	private async Task<IEnumerable<Guid>> Search(string value, CancellationToken token) {
		if (string.IsNullOrEmpty(value)) {
			return networks.Select(x => x.Id);
		}

		return networks
			.Where(x => x.Name.Contains(value, StringComparison.InvariantCultureIgnoreCase))
			.Select(x => x.Id);
	}
}