using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.Projects;

namespace Rise.Client.Components.Projects;

public partial class EditFormDialog {
	private readonly ProjectRequest.Edit.Validator _validator = new();
	private Result? _result;
	private MudForm form;

	[Parameter] public Guid ProjectId { get; set; }
	[Parameter] public required ProjectDto.Detail Project { get; set; }
	[CascadingParameter] private IMudDialogInstance MudDialog { get; set; }
	[Inject] private ISnackbar Snackbar { get; set; }
	[Inject] public required IProjectService ProjectService { get; set; }
	private ProjectRequest.Edit Model { get; set; }

	protected override async Task OnInitializedAsync() {
		if (Project is not null) {
			Model = new ProjectRequest.Edit {
				ProjectId = Project.Id,
				Name = Project.Name,
				Code = Project.Code,
				StartDate = new DateTime(
					Project.StartDate.Year,
					Project.StartDate.Month,
					Project.StartDate.Day),
				EndDate = Project.EndDate is { } endDate
					? new DateTime(endDate.Year, endDate.Month, endDate.Day)
					: null,
			};
		}
	}

	private void Cancel() => MudDialog.Cancel();

	private async Task EditProjectAsync() {
		await form.Validate();

		if (form.IsValid) {
			_result = await ProjectService.EditAsync(Model, CancellationToken.None);
			if (_result.IsSuccess) {
				Snackbar.Add("Project succesvol aangepast!", Severity.Success);
				MudDialog.Close(DialogResult.Ok(true));
			} else {
				Snackbar.Add(string.Join("\n", _result.Errors), Severity.Error);
			}
		}
	}
}