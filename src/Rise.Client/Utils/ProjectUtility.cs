using MudBlazor;
using Rise.Shared.Projects;

namespace Rise.Client.Utils;

/// <summary>
/// Provides helper methods to format and display information about a project.
/// </summary>
public static class ProjectUtility {
    /// <summary>
    /// Formats the start date in a long, human-readable format.
    /// </summary>
    /// <param name="startDate">The date when the project was started.</param>
    /// <returns>
    /// A string formatted as <c>d MMMM yyyy</c>.
    /// </returns>
    /// <example>
    /// 27 november 2025
    /// </example>
    public static string ToLongStartDate(DateOnly startDate)
        => startDate.ToString("d MMMM yyyy");

    /// <summary>
    /// Formats the start date in a short format.
    /// </summary>
    /// <param name="startDate">The date when the project was started.</param>
    /// <returns>
    /// A string formatted as <c>dd/MM/yyyy</c>.
    /// </returns>
    /// <example>
    /// 27/11/2025
    /// </example>
    public static string ToShortStartDate(DateOnly startDate)
        => startDate.ToString("dd/MM/yyyy");

    /// <summary>
    /// Formats the end date, if available, in a long, human-readable format.
    /// </summary>
    /// <param name="endDate">
    /// The date when the project was ended, or <c>null</c> if it is still active.
    /// </param>
    /// <returns>
    /// A string formatted as <c>d MMMM yyyy</c>, or <c>-</c> if <paramref name="endDate"/> is <c>null</c>.
    /// </returns>
    /// <example>
    /// 15 januari 2026
    /// </example>
    public static string ToLongEndDate(DateOnly? endDate)
        => endDate?.ToString("d MMMM yyyy") ?? "-";

    /// <summary>
    /// Formats the end date, if available, in a short format.
    /// </summary>
    /// <param name="endDate">
    /// The date when the project was ended, or <c>null</c> if it is still active.
    /// </param>
    /// <returns>
    /// A string formatted as <c>dd/MM/yyyy</c>, or <c>-</c> if <paramref name="endDate"/> is <c>null</c>.
    /// </returns>
    /// <example>
    /// 15/01/2026
    /// </example>
    public static string ToShortEndDate(DateOnly? endDate)
        => endDate?.ToString("dd/MM/yyyy") ?? "-";
    /// <summary>
    /// Returns a MudBlazor color based on the project status.
    /// </summary>
    /// <param name="status">The current status of the project.</param>
    /// <returns>
    /// <see cref="Color.Success"/> when the status is <see cref="Status.Actief"/>;  
    /// otherwise <see cref="Color.Error"/>.
    /// </returns>
    public static Color DefineStatusColor(Status status)
        => status == Status.Actief ? Color.Success : Color.Error;
}
