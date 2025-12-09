using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Services;
using Rise.Shared.Common;
using Rise.Shared.Locations;
using Rise.Shared.Stations;

namespace Rise.Client.Stations;

public partial class Create {
	private readonly MeasurementStationRequest.Create.Validator _validator = new();
	private Result<MeasurementStationResponse.Create>? _result;
	private MudForm _form;

	[Inject] private ISnackbar Snackbar { get; set; } = default!;
	[Inject] public required IMeasurementStationService StationService { get; set; }
	[Inject] public required NavigationManager NavigationManager { get; set; }
	[Inject] public required BreadcrumbService BreadcrumbService { get; set; }
	[Inject] public required HeaderService HeaderService { get; set; }

	private MeasurementStationRequest.Create Model { get; } = new() {
		IsNewLocation = false
	};

	protected override async Task OnInitializedAsync() {
		// set breadcrumbs
		BreadcrumbService.SetBreadcrumbs(
			new BreadcrumbItem("Meetstations", "/stations"),
			new BreadcrumbItem("Voeg toe", "/stations/create", true)
		);

		// set header
		HeaderService.Set(
			"Voeg toe",
			Icons.Material.Filled.ArrowBack,
			() => {
				NavigationManager.NavigateTo("/stations/");
				return Task.CompletedTask;
			}
		);
	}

	private async Task CreateProjectAsync() {
		await _form.Validate();

		if (_form.IsValid) {
			_result = await StationService.CreateAsync(Model);
			if (_result.IsSuccess) {
				Snackbar.Add("Meetstation succesvol aangemaakt!", Severity.Success);
				await Task.Delay(2000);
				NavigationManager.NavigateTo("/stations");
			} else {
				Snackbar.Add(string.Join("\n", _result.Errors), Severity.Error);
			}
		}
	}
}