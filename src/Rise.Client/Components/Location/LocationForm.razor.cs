using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Stations;
using Rise.Shared.Common;
using Rise.Shared.Locations;
using Rise.Shared.Stations;

namespace Rise.Client.Components.Location;

public partial class LocationForm {
	private IEnumerable<LocationDto.Index>? _locations;
	private string _searchText = string.Empty;
	private MudAutocomplete<Guid?>? _locationIdField;

	[Parameter] public Guid StationId { get; set; } = Guid.Empty;
	[Parameter] public required ILocationRequest Model { get; set; }
	[Inject] private ILocationService LocationService { get; set; } = default!;
	[Inject] private IDialogService DialogService { get; set; } = default!;

	protected override async Task OnInitializedAsync() {
		Result<LocationResponse.Index> locsResult = await LocationService.GetIndexAsync(new QueryRequest.SkipTake());
		_locations = locsResult.Value.Locations;

		if (StationId != Guid.Empty) {
			Model = new MeasurementStationRequest.UpdateLocation {
				MeasurementStationId = StationId
			};
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

	private async Task AddNewLocation() {
		Model.IsNewLocation = true;
		Model.LocationId = null;
		Model.Location = new LocationRequest.Create {
			Name = _searchText
		};

		await _locationIdField.ClearAsync();
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

		if (result is { Canceled: false, Data: CoordinateResult c }) {
			Model.Location.X = c.X;
			Model.Location.Y = c.Y;
		}
	}
}