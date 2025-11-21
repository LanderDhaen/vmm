using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Services;
using Rise.Shared.Common;
using Rise.Shared.Locations;
using Rise.Shared.Stations;

namespace Rise.Client.Stations;

public partial class Create {
	private Result<MeasurementStationResponse.Create>? _result;
	private IEnumerable<LocationDto.Index>? _locations;
	private string _searchText = string.Empty;
	[Inject] private ISnackbar Snackbar { get; set; }
	[Inject] public required IMeasurementStationService StationService { get; set; }
	[Inject] public required ILocationService LocationService { get; set; }
	[Inject] public required NavigationManager NavigationManager { get; set; }
	[Inject] public required BreadcrumbService BreadcrumbService { get; set; }
	[Inject] public required HeaderService HeaderService { get; set; }

	private MeasurementStationRequest.Create Model { get; } = new() {
		IsNewLocation = false
	};

	private readonly MeasurementStationRequest.Create.Validator _validator = new();
	MudForm form;

	protected override async Task OnInitializedAsync() {
		Result<LocationResponse.Index> result = await LocationService.GetIndexAsync(new QueryRequest.SkipTake());
		_locations = result.Value.Locations;

		// set breadcrumbs
		BreadcrumbService.SetBreadcrumbs(
			new BreadcrumbItem("Home", "/"),
			new BreadcrumbItem("Meetstations", "/stations"),
			new BreadcrumbItem("Voeg toe", $"/stations/create", true)
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
		await form.Validate();

		if (form.IsValid) {
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

	private async Task<IEnumerable<Guid?>> Search(string value, CancellationToken token) {
		if (string.IsNullOrEmpty(value)) {
			return _locations.Select(x => (Guid?)x.Id);
		}

		return _locations
			.Where(x => x.Name.Contains(value, StringComparison.InvariantCultureIgnoreCase))
			.Select(x => (Guid?)x.Id);
	}

	private void OnLocationSelected() {
		if (Model.LocationId.HasValue) {
			LocationDto.Index selected = _locations.FirstOrDefault(x => x.Id == Model.LocationId.Value);

			if (selected == null) return;

			Model.Location = new LocationRequest.Create {
				Name = selected.Name,
				Addressline1 = selected.Address.AddressLine1,
				Addressline2 = selected.Address.AddressLine2,
				City = selected.Address.City,
				PostalCode = selected.Address.PostalCode,
				CountryIso = selected.Address.CountryIso,
				X = selected.Coordinate.X,
				Y = selected.Coordinate.Y
			};
		} else {
			Model.Location = new LocationRequest.Create();
		}

		Model.IsNewLocation = false;
	}

	private void AddNewLocation() {
		Model.IsNewLocation = true;
		Model.LocationId = null;
		Model.Location = new LocationRequest.Create {
			Name = _searchText
		};
	}
}