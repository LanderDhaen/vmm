namespace Rise.Client.Utilities;

/// <summary>
/// Provides helper methods to format and display information about a measurement station.
/// </summary>

public static class StationUtility {
	/// <summary>
	/// Formats the installation date and time of a measurement station.
	/// </summary>
	/// <param name="installedAt">The date and time when the station was installed.</param>
	/// <returns>A string representing the installation date and time in the format "dd MMMM yyyy om HH:mm".</returns>
	/// <example>
	/// 27 november 2025 om 14:30
	/// </example>
	public static string FormatInstalledAt(DateTime installedAt)
		=> installedAt.ToString("dd MMMM yyyy 'om' HH:mm");

	/// <summary>
	/// Formats the removal date and time of a measurement station, if available.
	/// </summary>
	/// <param name="removedAt">The date and time when the station was removed, or <c>null</c> if it hasn't been removed.</param>
	/// <returns>A string representing the removal date and time in the format "dd MMMM yyyy om HH:mm", or "-" if <paramref name="removedAt"/> is <c>null</c>.</returns>
	/// <example>
	/// 15 januari 2026 om 09:00 (or "-")
	/// </example>
	public static string FormatRemovedAt(DateTime? removedAt)
		=> removedAt?.ToString("dd MMMM yyyy 'om' HH:mm") ?? "-";

}