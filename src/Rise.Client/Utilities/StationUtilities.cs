namespace Rise.Client.Utilities;

public static class StationUtilities {
	public static string FormatInstalledAt(DateTime installedAt) => installedAt.ToString("dd MMMM yyyy 'om' HH:mm");
	public static string FormatRemovedAt(DateTime? removedAt) => removedAt?.ToString("dd MMMM yyyy 'om' HH:mm") ?? "-";
}
