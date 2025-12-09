
using Microsoft.AspNetCore.Components;

using Rise.Shared.MaintObjects;

namespace Rise.Client.MaintObjects;

	public partial class ModificationItem {
	
		[Parameter] public required MaintObjDto.Modification Modification { get; init; }
		
		private static string GetTitle(MaintObjModificationType type)
		{
			return type switch
			{
				MaintObjModificationType.StatusChanged => "Status wijziging",
				MaintObjModificationType.Scanned => "Gescand",
				MaintObjModificationType.MovedToStock => "Verplaatst naar stock",
				MaintObjModificationType.AssignedToStation => "Toegevoegd aan station",
				_ => "Wijziging"
			};
		}
	}
