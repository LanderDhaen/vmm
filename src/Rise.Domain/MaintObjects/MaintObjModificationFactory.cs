using Rise.Domain.Projects;

namespace Rise.Domain.MaintObjects;
public static class MaintObjModificationFactory {
	public static List<MaintObjModification> Create(MaintObjHistory current, MaintObjHistory previous) {
		var modifications = new List<MaintObjModification>();

		if (current.MaintObjStatus != previous.MaintObjStatus) {
			modifications.Add(new MaintObjModification(
				MaintObjModificationType.StatusChanged,
				current.ValidityRange.ValidFrom,
				previous.MaintObjStatus.ToString(),
				current.MaintObjStatus.ToString()));
		}

		if (current.Location != previous.Location) {

			if (current.Location.IsStockLocation) {
				modifications.Add(new MaintObjModification(
					MaintObjModificationType.MovedToStock,
					current.ValidityRange.ValidFrom,
					$"{previous.Location.Name} ({previous.Station?.Code ?? "-"})",
					current.Location.Name
				));
			} else {
				modifications.Add(new MaintObjModification(
					MaintObjModificationType.AssignedToStation,
					current.ValidityRange.ValidFrom,
					previous.Location.Name,
					$"{current.Location.Name} ({current.Station?.Code ?? "-"})"
				));
			}
		}

		if (current.LastScannedBy?.Id != previous.LastScannedBy?.Id) {
			modifications.Add(new MaintObjModification(
				MaintObjModificationType.Scanned,
				current.ValidityRange.ValidFrom,
				DisplayName(previous.LastScannedBy),
				DisplayName(current.LastScannedBy)
			));
		}

		if (modifications.Count == 0) {
			throw new InvalidOperationException(
				"Niet mogelijk om de modificaties op te stellen voor deze twee historiek rijen");
		}

		return modifications;
	}

	private static string DisplayName(Technician? technician) =>
		technician is null ? "-" : $"{technician.FirstName} {technician.LastName}";
}