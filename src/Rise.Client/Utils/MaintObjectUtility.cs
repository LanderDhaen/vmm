using MudBlazor;

using Rise.Shared.MaintObjects;

namespace Rise.Client.Utils;

/// <summary>
/// Provides helper methods to format and display information about a maintenance object.
/// </summary>
public static class MaintObjectUtility {
	/// <summary>
	/// Returns a MudBlazor color based on the maintenance object status.
	/// </summary>
	/// <param name="status">The current status of the maintenance object.</param>
	/// <returns>
	/// <see cref="Color.Success"/> when the status is <see cref="MaintObjStatus.Actief"/>;  
	/// <see cref="Color.Error"/> when the status is <see cref="MaintObjStatus.Inactief"/>;  
	/// <see cref="Color.Warning"/> when the status is <see cref="MaintObjStatus.Defect"/>;  
	/// otherwise <see cref="Color.Default"/>.
	/// </returns>
	public static Color DefineStatusColor(MaintObjStatus status) {
		return status switch {
			MaintObjStatus.Actief => Color.Success,
			MaintObjStatus.Inactief => Color.Error,
			MaintObjStatus.Defect => Color.Warning,
			_ => Color.Default
		};
	}
}