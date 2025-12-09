using MudBlazor;

using Rise.Shared.MaintObjects;

namespace Rise.Client.Utils;

/// <summary>
/// Provides helper methods to format and display information about a maintenance object.
/// </summary>
public static class MaintObjectUtility {
	/// <summary>
	/// Translates the maintenance object type into a display-friendly label.
	/// </summary>
	/// <param name="maintObjType">The maintenance object type value.</param>
	/// <returns>A readable string representation of the maintenance object type.</returns>
	public static string TranslateType(MaintObjType maintObjType)
		=> maintObjType switch {
			MaintObjType.Battery => "Voeding",
			MaintObjType.Logger => "Logger",
			MaintObjType.Sensor => "Sonde",
			MaintObjType.Sim => "Sim",
			MaintObjType.Modem => "Modem",
			_ => string.Empty
		};

	/// <summary>
	/// Returns a MudBlazor color based on the maintenance object status.
	/// </summary>
	/// <param name="status">The current status of the maintenance object.</param>
	/// <returns>
	/// <see cref="Color.Success"/> when the status is <see cref="MaintObjStatus.Active"/>;  
	/// <see cref="Color.Error"/> when the status is <see cref="MaintObjStatus.Inactive"/>;  
	/// <see cref="Color.Warning"/> when the status is <see cref="MaintObjStatus.Broken"/>;  
	/// otherwise <see cref="Color.Default"/>.
	/// </returns>
	public static Color DefineStatusColor(MaintObjStatus status) {
		return status switch {
			MaintObjStatus.Active => Color.Success,
			MaintObjStatus.Inactive => Color.Error,
			MaintObjStatus.Broken => Color.Warning,
			_ => Color.Default
		};
	}

	/// <summary>
	/// Translates the maintenance object status into a display-friendly label.
	/// </summary>
	/// <param name="status">The current maintenance object status.</param>
	/// <returns>A readable string representation of the maintenance object status.</returns>
	public static string TranslateStatus(MaintObjStatus status)
		=> status switch {
			MaintObjStatus.Active => "Actief",
			MaintObjStatus.Inactive => "Inactief",
			MaintObjStatus.Broken => "Defect",
			_ => string.Empty
		};

	/// <summary>
	/// Formats the installation date and time in a long, human-readable format.
	/// </summary>
	/// <param name="installedAt">The installation date, or <c>null</c> if not installed.</param>
	/// <returns>
	/// A string formatted as <c>d MMMM yyyy HH'u'mm</c>, or <c>-</c> if <paramref name="removedAt"/> is <c>null</c>.
	/// </returns>
	/// <example>
	/// 27 november 2025 14u30
	/// </example>
	public static string ToLongInstalledAt(DateTime? installedAt)
		=> installedAt?.ToLocalTime().ToString("d MMMM yyyy HH'u'mm") ?? "-";

	/// <summary>
	/// Formats the installation date and time in a short format.
	/// </summary>
	/// <param name="installedAt">The installation date, or <c>null</c> if not installed.</param>
	/// <returns>
	/// A string formatted as <c>d MMMM yyyy HH'u'mm</c>, or <c>-</c> if <paramref name="installedAt"/> is <c>null</c>.
	/// </returns>
	/// <example>
	/// 15 januari 2026 09u00
	/// </example>
	public static string ToShortInstalledAt(DateTime? installedAt)
		=> installedAt?.ToLocalTime().ToString("dd/MM/yyyy HH'u'mm") ?? "-";

	/// <summary>
	/// Formats the removal date and time in a long, human-readable format.
	/// </summary>
	/// <param name="removedAt">
	/// The date and time when the maintenance object was removed, or <c>null</c> if it has not been removed.
	/// </param>
	/// <returns>
	/// A string formatted as <c>d MMMM yyyy HH'u'mm</c>, or <c>-</c> if <paramref name="removedAt"/> is <c>null</c>.
	/// </returns>
	/// <example>
	/// 15 januari 2026 09u00
	/// </example>
	public static string ToLongRemovedAt(DateTime? removedAt)
		=> removedAt?.ToLocalTime().ToString("d MMMM yyyy HH'u'mm") ?? "-";

	/// <summary>
	/// Formats the removal date and time in a short format.
	/// </summary>
	/// <param name="removedAt">The removal timestamp, or <c>null</c>.</param>
	/// <returns>
	/// A string formatted as <c>dd/MM/yyyy HH'u'mm</c>, or <c>-</c> if <paramref name="removedAt"/> is <c>null</c>.
	/// </returns>
	/// <example>
	/// 15/01/2026 09u00
	/// </example>
	public static string ToShortRemovedAt(DateTime? removedAt)
		=> removedAt?.ToLocalTime().ToString("dd/MM/yyyy HH'u'mm") ?? "-";

	/// <summary>
	/// Calculates and formats the next expected maintenance date.
	/// </summary>
	/// <param name="installedAt">The installation date, or <c>null</c> if not installed.</param>
	/// <param name="maintenanceInterval">The maintenance interval in days.</param>
	/// <returns>A string formatted as <c>d MMMM yyyy HH'u'mm</c>, or <c>Onderhoudsobject is niet geïnstalleerd</c> when <paramref name=\"installedAt\"/> is null.</returns>
	/// <example>27 november 2026 14u30</example>
	public static string CalculateNextMaintenance(DateTime? installedAt, int maintenanceInterval) =>
		installedAt.HasValue
			? installedAt.Value.AddDays(maintenanceInterval).ToString("d MMMM yyyy HH'u'mm")
			: "Onderhoudsobject is niet geïnstalleerd";

	/// <summary>
	/// Calculates and formats the next expected calibration date.
	/// </summary>
	/// <param name="installedAt">The installation date, or <c>null</c> if not installed.</param>
	/// <param name="calibrationInterval">The calibration interval in days.</param>
	/// <returns>A string formatted as <c>d MMMM yyyy HH'u'mm</c>, or <c>Onderhoudsobject is niet geïnstalleerd</c> when <paramref name="installedAt"/> is null.</returns>
	/// <example>12 maart 2026 10u15</example>
	public static string CalculateNextCalibration(DateTime? installedAt, int calibrationInterval) =>
		installedAt.HasValue
			? installedAt.Value.AddDays(calibrationInterval).ToString("d MMMM yyy HH'u'mm")
			: "Onderhoudsobject is niet geïnstalleerd";
}