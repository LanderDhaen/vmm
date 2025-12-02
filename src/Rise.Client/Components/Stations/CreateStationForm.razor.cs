using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Stations;
using Rise.Shared.Common;
using Rise.Shared.Locations;
using Rise.Shared.Stations;

namespace Rise.Client.Components.Stations;

public partial class CreateStationForm {
	private IEnumerable<LocationDto.Index>? _locations;
	private string _searchText = string.Empty;

	[Parameter] public required MeasurementStationRequest.Create Model { get; set; }
	[Parameter] public required MudForm Form { get; set; }

	[Inject] public required ILocationService LocationService { get; set; }
	[Inject] private IDialogService DialogService { get; set; } = default!;

	protected override async Task OnInitializedAsync() {
		Result<LocationResponse.Index> result = await LocationService.GetIndexAsync(new QueryRequest.SkipTake());
		_locations = result.Value.Locations;
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

			if (selected == null) {
				return;
			}

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

	private string BuildString(Guid? id) {
		LocationDto.Index? location = _locations!.FirstOrDefault(x => x.Id == id);

		if (location is null) {
			return string.Empty;
		}

		return $"{location.Name} ({location.Address.City})";
	}

	private async Task OpenMapDialog() {
		if (!Model.IsNewLocation || Model.Location is null) {
			return;
		}

		DialogParameters parameters = new() {
			["InitialX"] = Model.Location.X,
			["InitialY"] = Model.Location.Y
		};

		DialogOptions options = new() {
			CloseButton = true,
			MaxWidth = MaxWidth.Medium,
			FullWidth = true
		};

		IDialogReference dialog = await DialogService.ShowAsync<PickLocationDialog>(
			"Kies meetstation locatie",
			parameters,
			options
		);

		DialogResult? result = await dialog.Result;

		if (!result.Canceled && result.Data is CoordinateResult c) {
			Model.Location.X = c.X;
			Model.Location.Y = c.Y;
		}
	}
}