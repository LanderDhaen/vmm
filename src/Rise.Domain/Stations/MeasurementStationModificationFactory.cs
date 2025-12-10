namespace Rise.Domain.Stations {
	/// <summary>
	///     Factory responsible for creating <see cref="MeasurementStationModification"/> objects
	///     from a pair of <see cref="MeasurementStationHistory"/> entries.
	/// </summary>
	public static class MeasurementStationModificationFactory {
		public static List<MeasurementStationModification> Create(
			MeasurementStationHistory current,
			MeasurementStationHistory previous) {
			
			List<MeasurementStationModification> modifications = [];
			
			if (!current.Location.Equals(previous.Location)) {
				modifications.Add(new MeasurementStationModification(
					MeasurementStationModificationType.LocationChanged,
					current.ValidityRange.ValidFrom,
					previous.Location.Name,
					current.Location.Name
				));
			}
			
			if (!current.Installation.RemovedAt.Equals(previous.Installation.RemovedAt)) {
				modifications.Add( new MeasurementStationModification(
					MeasurementStationModificationType.RemovedAtChanged,
					current.ValidityRange.ValidFrom,
					previous.Installation.RemovedAt?.ToString("s") ?? "-",
					current.Installation.RemovedAt?.ToString("s") ?? "-"
				));
			}

			if (!current.Installation.InstalledAt.Equals(previous.Installation.InstalledAt)) {
				modifications.Add(new MeasurementStationModification(
					MeasurementStationModificationType.InstalledAtChanged,
					current.ValidityRange.ValidFrom,
					previous.Installation.InstalledAt.ToString("s"),  
					current.Installation.InstalledAt.ToString("s")
				));
			}
			
			if (modifications.Count == 0) {
				throw new InvalidOperationException("Niet mogelijk om een modificatie op te stellen voor deze twee historiekrijen");
			}

			return modifications;
		}
	}
}