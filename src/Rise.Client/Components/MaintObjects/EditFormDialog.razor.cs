using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.MaintObjects;

namespace Rise.Client.Components.MaintObjects;

public partial class EditFormDialog {
	private readonly MaintObjRequest.Edit.Validator _validator = new();
	private Result? _result;
	private MudForm form;

	[Parameter] public Guid MaintObjId { get; set; }
	[Parameter] public MaintObjDto.Detail MaintObj { get; set; }
	[CascadingParameter] private IMudDialogInstance MudDialog { get; set; }
	[Inject] private ISnackbar Snackbar { get; set; }
	[Inject] public required IMaintObjService MaintObjService { get; set; }
	private MaintObjRequest.Edit Model { get; set; }

	protected override async Task OnInitializedAsync() {
		if (MaintObj is not null) {
			Model = new MaintObjRequest.Edit {
				MaintObjId = MaintObj.Id,
				Name = MaintObj.Name,
				SerialNumber = MaintObj.SerialNumber,
				MaintObjType = MaintObj.MaintObjType,
				Brand = MaintObj.Brand,
				InstalledAt = MaintObj.InstalledAt,
				RemovedAt = MaintObj.RemovedAt
			};
		}
	}

	private void Cancel() {
		MudDialog.Cancel();
	}

	private async Task EditMaintObjAsync() {
		await form.Validate();

		if (form.IsValid) {
			_result = await MaintObjService.EditAsync(Model, CancellationToken.None);
			if (_result.IsSuccess) {
				Snackbar.Add("Onderhoudsobject succesvol aangepast!", Severity.Success);
				MudDialog.Close(DialogResult.Ok(true));
			} else {
				Snackbar.Add(string.Join("\n", _result.Errors), Severity.Error);
			}
		}
	}
}