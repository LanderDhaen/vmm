using Microsoft.AspNetCore.Components;

using Rise.Shared.Stations;

namespace Rise.Client.Stations;

public partial class History {
	private MeasurementStationDto.History? _measurementStation;

	[Parameter] public Guid MeasurementStationId { get; set; }
	[Inject] public required IMeasurementStationService MeasurementStationService { get; set; }
	

	protected override async Task OnInitializedAsync() {
		Result<MeasurementStationResponse.History> result =
			await MeasurementStationService.GetHistoryAsync(MeasurementStationId);

		if (result.IsSuccess) {
			_measurementStation = result.Value.MeasurementStation;
		}
	}

	private string CompareHistories(MeasurementStationHistoryDto.Index? prev, MeasurementStationHistoryDto.Index next) {

		if (prev is null) {
			return "";
		}
		
		if (!prev.Location.Equals(next.Location)) {
			return $"Meetstation verplaatst van {prev.Location} naar {next.Location}";
		}

		if (!prev.InstalledAt.Equals(next.InstalledAt)) {
			return $"Installeren van meetstation gewijzigd van {prev.InstalledAt} naar {next.InstalledAt}";
		}

		if (!prev.RemovedAt.Equals(next.RemovedAt)) {
			return $"Verwijderen van meetstation gewijzigd van {prev.InstalledAt} naar {next.RemovedAt}";
		}
		
		return "";
	}
}