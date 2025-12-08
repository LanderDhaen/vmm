using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Stations;
using Rise.Shared.Common;
using Rise.Shared.Locations;

namespace Rise.Client.Components.Locations;

public partial class LocationForm {
	private readonly MudAutocomplete<Guid?>? _locationIdField;
	private IEnumerable<LocationDto.Index>? _locations;
	private CancellationTokenSource _cancellationTokenSource = new();
	private string _searchTerm = string.Empty;

	[Parameter] public Guid StationId { get; set; } = Guid.Empty;
	[Parameter] public required ILocationRequest Model { get; set; }
	[Inject] private ILocationService LocationService { get; set; } = default!;
	[Inject] private IDialogService DialogService { get; set; } = default!;

	protected override async Task OnInitializedAsync() {
		await RefreshLocations();
	}

	private async Task RefreshLocations() {
		await _cancellationTokenSource.CancelAsync();
		_cancellationTokenSource.Dispose();
		_cancellationTokenSource = new CancellationTokenSource();

		QueryRequest.SkipTake request = new() {
			SearchTerm = _searchTerm
		};
		request.Filters["isStockLocation"] = false;

		Result<LocationResponse.Index> locsResult = await LocationService.GetIndexAsync(request, _cancellationTokenSource.Token);

		if (locsResult.IsSuccess) {
			_locations = locsResult.Value.Locations;
		}
	}

	private async Task<IEnumerable<Guid?>> Search(string value, CancellationToken token) {
		_searchTerm = value;
		await RefreshLocations();

		return _locations?.Select(l => (Guid?)l.Id) ?? [];
	}

	private void OnLocationSelected() => Model.IsNewLocation = false;

	private async Task AddNewLocation() {
		Model.IsNewLocation = true;
		Model.LocationId = null;
		Model.Location = new LocationRequest.Create();

		await _locationIdField.ClearAsync();
	}

	private string BuildString(Guid? id) {
		LocationDto.Index? location = _locations!.FirstOrDefault(x => x.Id == id);

		return location is null ? string.Empty : $"{location.Name} ({location.Address.City})";
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