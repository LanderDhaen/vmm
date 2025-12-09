namespace Rise.Client.Utils;

/// <summary>
/// Provides helper methods to format and display date and time information for measurement stations.
/// </summary>
public static class StationUtility {
	/// <summary>
	/// Formats the installation date and time in a long, human-readable format.
	/// </summary>
	/// <param name="installedAt">The date and time when the station was installed.</param>
	/// <returns>
	/// A string formatted as <c>d MMMM yyyy HH'u'mm</c>.
	/// </returns>
	/// <example>
	/// 27 november 2025 14u30
	/// </example>
	public static string ToLongInstalledAt(DateTime installedAt)
		=> installedAt.ToLocalTime().ToString("d MMMM yyyy HH'u'mm");

	/// <summary>
	/// Formats the installation date and time in a short format.
	/// </summary>
	/// <param name="installedAt">The date and time when the station was installed.</param>
	/// <returns>
	/// A string formatted as <c>dd/MM/yyyy HH'u'mm</c>.
	/// </returns>
	/// <example>
	/// 27/11/2025 14u30
	/// </example>
	public static string ToShortInstalledAt(DateTime installedAt)
		=> installedAt.ToLocalTime().ToString("dd/MM/yyyy HH'u'mm");

	/// <summary>
	/// Formats the removal date and time, if available, in a long, human-readable format.
	/// </summary>
	/// <param name="removedAt">
	/// The date and time when the station was removed, or <c>null</c> if it has not been removed.
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
	/// Formats the removal date and time, if available, in a short format.
	/// </summary>
	/// <param name="removedAt">
	/// The date and time when the station was removed, or <c>null</c> if it has not been removed.
	/// </param>
	/// <returns>
	/// A string formatted as <c>dd/MM/yyyy HH'u'mm</c>, or <c>-</c> if <paramref name="removedAt"/> is <c>null</c>.
	/// </returns>
	/// <example>
	/// 15/01/2026 09u00
	/// </example>
	public static string ToShortRemovedAt(DateTime? removedAt)
		=> removedAt?.ToLocalTime().ToString("dd/MM/yyyy HH'u'mm") ?? "-";
}