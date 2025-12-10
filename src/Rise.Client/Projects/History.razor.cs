using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.Projects;

namespace Rise.Client.Projects;

public partial class History {
	private IEnumerable<ProjectDto.Modification>? _modifications;

	[Parameter] public Guid ProjectId { get; set; }
	[Inject] public required IProjectService ProjectService { get; set; }

	protected override async Task OnInitializedAsync() {
		Result<ProjectResponse.History> result = await ProjectService.GetHistoryAsync(ProjectId);

		if (result.IsSuccess) {
			_modifications = result.Value.Modifications;
		}
	}
}