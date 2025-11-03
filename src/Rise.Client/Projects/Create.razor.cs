using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.Common;
using Rise.Shared.Networks;
using Rise.Shared.Projects;

namespace Rise.Client.Projects;

public partial class Create {
	private Result<ProjectResponse.Create>? _result;
	private IEnumerable<MeasurementNetworkDto.Index>? networks;
	[Inject] public required IProjectService ProjectService { get; set; }
	[Inject] public required INetworkService NetworkService { get; set; }
	[Inject] public required NavigationManager NavigationManager { get; set; }
	private ProjectRequest.Create Model { get; } = new();
	private readonly ProjectRequest.Create.Validator validator = new();
	MudForm form;

	protected override async Task OnInitializedAsync() {
		QueryRequest.SkipTake request = new() {
			Skip = 0,
			Take = 50,
			OrderBy = "Id"
		};

		Result<NetworkResponse.Index> result = await NetworkService.GetIndexAsync(request);
		networks = result.Value.measurementsNetworks;
	}

	private async Task CreateProjectAsync() {
		await form.Validate();

		if (form.IsValid) {
			_result = await ProjectService.CreateAsync(Model);
			if (_result.IsSuccess) {
				NavigationManager.NavigateTo("/projects");
			}
		}
	}
}