using MudBlazor;
using Rise.Shared.Projects;

namespace Rise.Client.Utils;

/// <summary>
/// Provides helper methods to format and display information about a project.
/// </summary>
public static class ProjectUtility
{
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