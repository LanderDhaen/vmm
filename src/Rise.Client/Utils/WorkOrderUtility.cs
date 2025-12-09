using MudBlazor;

using Rise.Shared.Projects;
using Rise.Shared.WorkOrders;

namespace Rise.Client.Utils;

/// <summary>
/// Provides helper methods to format and display information related to work orders.
/// </summary>
public static class WorkOrderUtility {
	/// <summary>
	/// Returns a MudBlazor color based on the work order status.
	/// </summary>
	/// <param name="status">The current work order status.</param>
	/// <returns>
	/// A color representing the status, such as <see cref="Color.Success"/> for completed work orders.
	/// </returns>
	public static Color DefineStatusColor(WorkOrderStatus status)
		=> status switch {
			WorkOrderStatus.Pending => Color.Dark,
			WorkOrderStatus.InProgress => Color.Info,
			WorkOrderStatus.Completed => Color.Success,
			WorkOrderStatus.Cancelled => Color.Error,
			_ => Color.Default
		};

	/// <summary>
	/// Translates the work order status into a display-friendly label.
	/// </summary>
	/// <param name="status">The current work order status.</param>
	/// <returns>A readable string representation of the status.</returns>
	public static string TranslateStatus(WorkOrderStatus status)
		=> status switch {
			WorkOrderStatus.Pending => "Gepland",
			WorkOrderStatus.InProgress => "Opgenomen",
			WorkOrderStatus.Completed => "Voltooid",
			WorkOrderStatus.Cancelled => "Geannuleerd",
			_ => string.Empty
		};

	/// <summary>
	/// Returns a MudBlazor color based on priority level.
	/// </summary>
	/// <param name="priority">The work order priority.</param>
	/// <returns>
	/// A color representing the priority, such as <see cref="Color.Error"/> for high priority.
	/// </returns>
	public static Color DefinePriorityColor(Priority priority)
		=> priority switch {
			Priority.High => Color.Error,
			Priority.Normal => Color.Warning,
			Priority.Low => Color.Info,
			_ => Color.Default
		};

	/// <summary>
	/// Returns a MudBlazor Icon representing the priority level.
	/// </summary>
	/// <param name="priority">The work order priority.</param>
	/// <returns>An icon identifier suitable for display.</returns>
	public static string DefinePriorityIcon(Priority priority)
		=> priority switch {
			Priority.High => Icons.Material.Filled.KeyboardArrowUp,
			Priority.Normal => Icons.Material.Filled.HorizontalRule,
			Priority.Low => Icons.Material.Filled.KeyboardArrowDown,
			_ => Icons.Material.Filled.HorizontalRule
		};

	/// <summary>
	/// Translates the priority level into a display-friendly label.
	/// </summary>
	/// <param name="priority">The priority value.</param>
	/// <returns>A readable string representation of the priority.</returns>
	public static string TranslatePriority(Priority priority)
		=> priority switch {
			Priority.High => "Hoog",
			Priority.Normal => "Normaal",
			Priority.Low => "Laag",
			_ => string.Empty
		};

	/// <summary>
	/// Formats the created at date and time in a long, human-readable format.
	/// </summary>
	/// <param name="createdAt">The date and time when the work order was created.</param>
	/// <returns>
	/// A string formatted as <c>d MMMM yyyy HH'u'mm</c>.
	/// </returns>
	/// <example>
	/// 27 november 2025 14u30
	/// </example>
	public static string ToLongCreatedAt(DateTime createdAt)
		=> createdAt.ToLocalTime().ToString("d MMMM yyyy HH'u'mm");

	/// <summary>
	/// Formats the created at date and time in a short format.
	/// </summary>
	/// <param name="createdAt">The date and time when the work order was created.</param>
	/// <returns>
	/// A string formatted as <c>dd/MM/yyyy HH'u'mm</c>.
	/// </returns>
	/// <example>
	/// 27/11/2025 14u30
	/// </example>
	public static string ToShortCreatedAt(DateTime createdAt)
		=> createdAt.ToLocalTime().ToString("dd/MM/yyyy HH'u'mm");

	/// <summary>
	/// Formats the start date, if available, in a long, human-readable format.
	/// </summary>
	/// <param name="startDate">The date when the work order was claimed by a technician.</param>
	/// <returns>
	/// A string formatted as <c>d MMMM yyyy</c>.
	/// </returns>
	/// <example>
	/// 27 november 2025
	/// </example>
	public static string ToLongStartDate(DateOnly? startDate)
		=> startDate?.ToString("d MMMM yyyy") ?? "-";
	
	/// <summary>
	/// Formats the start date, if available, in a short format.
	/// </summary>
	/// <param name="startDate">The date when the work order was claimed by a technician.</param>
	/// <returns>
	/// A string formatted as <c>dd/MM/yyyy</c>.
	/// </returns>
	/// <example>
	/// 27/11/2025
	/// </example>
	public static string ToShortStartDate(DateOnly? startDate)
		=> startDate?.ToString("dd/MM/yyyy") ?? "-";
	
	/// <summary>
	/// Formats the end date, if available, in a human-readable format.
	/// </summary>
	/// <param name="endDate">
	/// The date when the work order was completed/canceled, or <c>null</c> if it is still in progress.
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
	/// The date when the work order was completed/canceled, or <c>null</c> if it is still in progress.
	/// </param>
	/// <returns>
	/// A string formatted as <c>dd/MM/yyyy</c>, or <c>-</c> if <paramref name="endDate"/> is <c>null</c>.
	/// </returns>
	/// <example>
	/// 15/01/2026
	/// </example>
	public static string ToShortEndDate(DateOnly? endDate)
		=> endDate?.ToString("dd/MM/yyyy") ?? "-";
}