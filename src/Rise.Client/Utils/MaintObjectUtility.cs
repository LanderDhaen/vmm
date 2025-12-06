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
}