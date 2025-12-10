using Microsoft.AspNetCore.Components;

using Rise.Client.Utils;
using Rise.Shared.Projects;

namespace Rise.Client.Projects;

public partial class ModificationItem {

	[Parameter] public required ProjectDto.Modification Modification { get; init; }

	private static string GetTitle(ProjectModificationType modificationType) {
		return modificationType switch {
			ProjectModificationType.StartDateChanged => "Startdatum gewijzigd",
			ProjectModificationType.EndDateChanged => "Einddatum gewijzigd",
			ProjectModificationType.StatusChanged => "Status gewijzigd",
			ProjectModificationType.MeasurementNetworkChanged => "Meetnet gewijzigd",
			_ => "Wijziging"
		};
	}
	
	private static string ParseStartDate(string date) 
		=> DateOnly.TryParse(date, out DateOnly parsedDate) ? ProjectUtility.ToLongStartDate(parsedDate) : date;
	
	private static string ParseEndDate(string date) 
		=> DateOnly.TryParse(date, out DateOnly parsedDate) ? ProjectUtility.ToLongEndDate(parsedDate) : date;
	
}