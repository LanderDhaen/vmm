using Microsoft.AspNetCore.Components;

using Rise.Client.Utils;
using Rise.Shared.Stations;

namespace Rise.Client.Stations;

public partial class ModificationItem {

	[Parameter] public required MeasurementStationDto.Modification Modification { get; init; }

	private static string GetTitle(MeasurementStationModificationType modificationType) {
		return modificationType switch {
			MeasurementStationModificationType.LocationChanged => "Locatie gewijzigd",
			MeasurementStationModificationType.InstalledAtChanged => "Installatiedatum gewijzigd",
			MeasurementStationModificationType.RemovedAtChanged => "Verwijderdatum gewijzigd",
			_ => "Wijziging"
		};
	}
	
	private static string ParseInstalledAt(string datetime) 
		=> DateTime.TryParse(datetime, out DateTime parsedDateTime) ? StationUtility.ToLongInstalledAt(parsedDateTime) : datetime;
	
	private static string ParseRemovedAt(string datetime) 
		=> DateTime.TryParse(datetime, out DateTime parsedDateTime) ? StationUtility.ToLongRemovedAt(parsedDateTime) : datetime;
	
}