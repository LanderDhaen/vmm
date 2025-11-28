using MudBlazor;

using Rise.Shared.Projects;

namespace Rise.Client.Utilities;

/// <summary>
/// Provides helper methods to format and display information about a project.
/// </summary>

public static class ProjectUtility
{
	/// <summary>
	/// Formats the installation date and time of a measurement station.
	/// </summary>
	/// <param name="installedAt">The date and time when the station was installed.</param>
	/// <returns>A string representing the installation date and time in the format "dd MMMM yyyy om HH:mm".</returns>
	/// <example>
	/// 27 november 2025 om 14:30
	/// </example>
	public static Color DefineStatusColor(Status status) 
		=> status == Status.Actief ? Color.Success : Color.Error;
}