using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.Common;
using Rise.Shared.Networks;
using Rise.Shared.Projects;

namespace Rise.Client.Components.Projects;

public partial class UpdateMeasurementNetworkDialog {
	private readonly ProjectRequest.UpdateMeasurementNetwork.Validator _validator = new();
	private IEnumerable<MeasurementNetworkDto.Index>? _networks;
	private Result? _result;
	private MudForm form;
	[Parameter] public Guid ProjectId { get; set; }
	[Parameter] public Guid MeasurementNetworkId { get; set; }
	[CascadingParameter] private IMudDialogInstance MudDialog { get; set; }
	[Inject] private ISnackbar Snackbar { get; set; }
	[Inject] public required IProjectService ProjectService { get; set; }
	[Inject] public required INetworkService NetworkService { get; set; }
	private ProjectRequest.UpdateMeasurementNetwork Model { get; set; }

	protected override async Task OnInitializedAsync() {
		QueryRequest.SkipTake request = new() {
			Skip = 0,
			Take = Int32.MaxValue
		};
		
		Result<NetworkResponse.Index> result = await NetworkService.GetIndexAsync(request);
		_networks = result.Value.measurementsNetworks;

		Model = new ProjectRequest.UpdateMeasurementNetwork {
			ProjectId = ProjectId,
			MeasurementNetworkId = MeasurementNetworkId
		};
	}

	private void Cancel() => MudDialog.Cancel();

	private async Task UpdateMeasurementNetworkAsync() {
		await form.Validate();

		if (form.IsValid) {
			_result = await ProjectService.UpdateMeasurementNetworkAsync(Model, CancellationToken.None);
			if (_result.IsSuccess) {
				Snackbar.Add("Meetnet succesvol aangepast!", Severity.Success);
				MudDialog.Close(DialogResult.Ok(true));
			} else {
				Snackbar.Add(string.Join("\n", _result.Errors), Severity.Error);
			}
		}
	}
}