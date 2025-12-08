using Microsoft.AspNetCore.Components;

using MudBlazor;
using Rise.Shared.Projects;

namespace Rise.Client.Projects;

public partial class History {
	private IEnumerable<ProjectDto.Modification>? _modifications;

	[Parameter] public Guid ProjectId { get; set; }
	[Inject] public required IProjectService ProjectService { get; set; }
	
	private TimelinePosition _timelinePosition;
	
	protected override async Task OnInitializedAsync() {
		Result<ProjectResponse.History> result = await ProjectService.GetHistoryAsync(ProjectId);

		if (result.IsSuccess) {
			_modifications = result.Value.Modifications;
		}
	}
	
	private void OnBreakPointChange(Breakpoint breakpoint) => _timelinePosition = breakpoint == Breakpoint.Xs ? TimelinePosition.Left : TimelinePosition.Alternate;

	private static string DefineTitle(ProjectModificationType modificationType) {
		return modificationType switch {
			ProjectModificationType.StartDateChanged => "Startdatum gewijzigd",
			ProjectModificationType.EndDateChanged => "Einddatum gewijzigd",
			_ => "Wijziging"
		};
	}
}