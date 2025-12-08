using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.Projects;
using Rise.Shared.Stations;

namespace Rise.Client.Components.Stations;

public partial class EditFormDialog {
	private readonly MeasurementStationRequest.Edit.Validator _validator = new();
	private Result? _result;
	private MudForm form;

	[Parameter] public Guid StationId { get; set; }
	[Parameter] public MeasurementStationDto.Detail Station { get; set; }
	[CascadingParameter] private IMudDialogInstance MudDialog { get; set; }
	[Inject] private ISnackbar Snackbar { get; set; }
	[Inject] public required IMeasurementStationService StationService { get; set; }
	private MeasurementStationRequest.Edit Model { get; set; }

	protected override async Task OnInitializedAsync() {
		if (Station is not null) {
			Model = new MeasurementStationRequest.Edit {
				StationId = Station.Id,
				Code = Station.Code,
				InstalledAt = Station.InstalledAt,
				RemovedAt = Station.RemovedAt
			};
		}
	}

	private void Cancel() => MudDialog.Cancel();

	private async Task EditStationAsync() {
		await form.Validate();

		if (form.IsValid) {
			_result = await StationService.EditAsync(Model, CancellationToken.None);
			if (_result.IsSuccess) {
				Snackbar.Add("Meetstation succesvol aangepast!", Severity.Success);
				MudDialog.Close(DialogResult.Ok(true));
			} else {
				Snackbar.Add(string.Join("\n", _result.Errors), Severity.Error);
			}
		}
	}
}