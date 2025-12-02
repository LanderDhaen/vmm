namespace Rise.Client.Utils;

/// <summary>
/// Provides helper methods to format and display date and time information for histories.
/// </summary>
public static class HistoryUtility
{
    /// <summary>
    /// Formats the ValidFrom date in a long, human-readable format.
    /// </summary>
    /// <param name="validFrom">The start of the validity range.</param>
    /// <returns>A string formatted as <c>d MMMM yyyy 'om' HH'u'mm</c>.</returns>
    /// <example>
    /// 27 november 2025 om 14u30
    /// </example>
    public static string ToLongValidFrom(DateTime validFrom)
        => validFrom.ToLocalTime().ToString("d MMMM yyyy 'om' HH'u'mm");

    /// <summary>
    /// Formats the ValidFrom date in a short format.
    /// </summary>
    /// <param name="validFrom">The start of the validity range.</param>
    /// <returns>A string formatted as <c>dd/MM/yyyy 'om' HH'u'mm</c>.</returns>
    /// <example>
    /// 27/11/2025 om 14u30
    /// </example>
    public static string ToShortValidFrom(DateTime validFrom)
        => validFrom.ToLocalTime().ToString("dd/MM/yyyy 'om' HH'u'mm");

    /// <summary>
    /// Formats the ValidTo date in a long format, or returns "nu" if null.
    /// </summary>
    /// <param name="validTo">The end of the validity range, or <c>null</c> if current.</param>
    /// <returns>A string formatted as <c>dd MMMM yyyy 'om' HH'u'mm</c>, or <c>nu</c> if <paramref name="validTo"/> is <c>null</c>.</returns>
    /// <example>
    /// 15 januari 2026 om 09u00
    /// </example>
    public static string ToLongValidTo(DateTime? validTo)
        => validTo?.ToLocalTime().ToString("d MMMM yyyy 'om' HH'u'mm") ?? "nu";

    /// <summary>
    /// Formats the ValidTo date in a short format, or returns "nu" if null.
    /// </summary>
    /// <param name="validTo">The end of the validity range, or <c>null</c> if current.</param>
    /// <returns>A string formatted as <c>d/MM/yyyy 'om' HH'u'mm</c>, or <c>nu</c> if <paramref name="validTo"/> is <c>null</c>.</returns>
    /// <example>
    /// 15/01/2026 om 09u00
    /// </example>
    public static string ToShortValidTo(DateTime? validTo)
        => validTo?.ToLocalTime().ToString("dd/MM/yyyy 'om' HH'u'mm") ?? "nu";
}
