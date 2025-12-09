using Rise.Domain.Projects;

namespace Rise.Domain.MaintObjects;

public static class MaintObjModificationFactory {
	public static MaintObjModification Create(MaintObjHistory current, MaintObjHistory previous) {

		if (current.MaintObjStatus != previous.MaintObjStatus) {
			return new MaintObjModification(MaintObjModificationType.StatusChanged,
				current.ValidityRange.ValidFrom,
				previous.MaintObjStatus.ToString(),
				current.MaintObjStatus.ToString());
		}
		
		if (current.Location != previous.Location) {

			if (current.Location.IsStockLocation) {
				return new MaintObjModification(MaintObjModificationType.MovedToStock,
					current.ValidityRange.ValidFrom,
					previous.Location.Name,
					current.Location.Name
				);	
			}
			
			return new MaintObjModification(MaintObjModificationType.AssignedToStation, 
				current.ValidityRange.ValidFrom,
					previous.Station?.Code ?? "-",
					current.Station?.Code ?? "-"
				);
		}

		if (current.LastScannedBy?.Id != previous.LastScannedBy?.Id) {
			return new MaintObjModification(MaintObjModificationType.Scanned,
				current.ValidityRange.ValidFrom,
				DisplayName(previous.LastScannedBy), 
				DisplayName(current.LastScannedBy));
		}
		
		throw new InvalidOperationException("Niet mogelijk om een modificatie op te stellen voor deze twee historiek rijen");
	}
	
	// Helper
	
	private static string DisplayName(Technician? technician) =>
		technician is null ? "-" : $"{technician.FirstName} {technician.LastName}";

}

