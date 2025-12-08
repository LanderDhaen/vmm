using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Services;
using Rise.Shared.Stations;

using Severity = MudBlazor.Severity;

namespace Rise.Client.Stations;

public partial class Edit {
	private readonly MeasurementStationRequest.Edit.Validator _validator = new();
	private MudForm _form = null!;

	[Parameter] public Guid StationId { get; set; }
	[Inject] private ISnackbar Snackbar { get; set; } = null!;
	[Inject] private IMeasurementStationService StationService { get; set; } = null!;
	[Inject] private NavigationManager Nav { get; set; } = null!;
	[Inject] public required BreadcrumbService BreadcrumbService { get; set; }
	[Inject] public required HeaderService HeaderService { get; set; }

	private MeasurementStationRequest.Edit Model { get; set; } = new();

	protected override async Task OnInitializedAsync() {
		Result<MeasurementStationResponse.Detail> detail = await StationService.GetDetailAsync(StationId);

		if (detail.IsSuccess) {
			Model = new MeasurementStationRequest.Edit {
				StationId = StationId,
				Code = detail.Value.Station.Code,
				InstalledAt = detail.Value.Station.InstalledAt,
				RemovedAt = detail.Value.Station.RemovedAt
			};
		}


		BreadcrumbService.SetBreadcrumbs(
			new BreadcrumbItem("Meetstations", "/stations"),
			new BreadcrumbItem($"{detail.Value.Station.Code}", $"/stations/{StationId}"),
			new BreadcrumbItem("Bewerk", $"/stations/edit/{StationId}", true)
		);

		// set header
		HeaderService.Set(
			"Bewerk",
			Icons.Material.Filled.ArrowBack,
			() => {
				Navigation.NavigateTo("/stations/");
				return Task.CompletedTask;
			}
		);
	}

	private async Task EditStationAsync() {
		await _form.Validate();

		if (_form.IsValid) {
			Result result = await StationService.EditAsync(Model, CancellationToken.None);
			if (result.IsSuccess) {
				Snackbar.Add("Meetstation succesvol aangepast!", Severity.Success);
				await Task.Delay(1200);
				Nav.NavigateTo("/stations");
			} else {
				Snackbar.Add(string.Join("\n", result.Errors), Severity.Error);
			}
		}
	}
}