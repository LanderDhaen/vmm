using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.Stations;

namespace Rise.Client.Components.Stations;

public partial class UpdateLocationDialog {
	private readonly MeasurementStationRequest.UpdateLocation.Validator _validator = new();
	private Result? _result;
	private MudForm form;
	[Parameter] public Guid StationId { get; set; }
	[Parameter] public Guid LocationId { get; set; }
	[CascadingParameter] private IMudDialogInstance MudDialog { get; set; }
	[Inject] private ISnackbar Snackbar { get; set; }
	[Inject] public required IMeasurementStationService StationService { get; set; }
	private MeasurementStationRequest.UpdateLocation Model { get; set; }

	protected override async Task OnInitializedAsync() {
		Model = new MeasurementStationRequest.UpdateLocation() {
			MeasurementStationId = StationId,
		};
	}

	private void Cancel() => MudDialog.Cancel();

	private async Task UpdateLocationAsync() {
		await form.Validate();

		if (form.IsValid) {
			Console.WriteLine(Model.LocationId);
			_result = await StationService.UpdateLocationAsync(Model, CancellationToken.None);
			if (_result.IsSuccess) {
				Snackbar.Add("Locatie succesvol aangepast!", Severity.Success);
				MudDialog.Close(DialogResult.Ok(true));
			} else {
				Snackbar.Add(string.Join("\n", _result.Errors), Severity.Error);
			}
		}
	}
}