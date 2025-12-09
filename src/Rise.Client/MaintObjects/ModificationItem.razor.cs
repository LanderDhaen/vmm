
using Microsoft.AspNetCore.Components;

using Rise.Shared.MaintObjects;

namespace Rise.Client.MaintObjects;

	public partial class ModificationItem {
	
		[Parameter] public required MaintObjDto.Modification Modification { get; init; }
		
		private static string GetTitle(MaintObjModificationType type)
		{
			return type switch
			{
				MaintObjModificationType.StatusChanged => "Status gewijzigd",
				MaintObjModificationType.Scanned => "Gescand",
				MaintObjModificationType.MovedToStock => "Locatie gewijzigd",
				MaintObjModificationType.AssignedToStation => "Locatie gewijzigd",
				_ => "Wijziging"
			};
		}
	}
