namespace Rise.Domain.Projects;
/// <summary>
///     Factory responsible for creating <see cref="ProjectModification"/> objects
///     from a pair of <see cref="ProjectHistory"/> entries.
/// </summary>
public static class ProjectModificationFactory {
	public static ProjectModification Create(ProjectHistory current, ProjectHistory previous) {
		
		if (!current.DateRange.StartDate.Equals(previous.DateRange.StartDate)) {
			return new ProjectModification(ProjectModificationType.StartDateChanged, 
				current.ValidityRange.ValidFrom, 
				previous.DateRange.StartDate.ToString(),
				current.DateRange.StartDate.ToString());
		}

		if (!current.DateRange.EndDate.Equals(previous.DateRange.EndDate)) {
			return new ProjectModification(ProjectModificationType.EndDateChanged, 
				current.ValidityRange.ValidFrom, 
				previous.DateRange.EndDate?.ToString() ?? "-",
				current.DateRange.EndDate?.ToString() ?? "-");
		}

		if (!current.Status.Equals(previous.Status)) {
			return new ProjectModification(ProjectModificationType.StatusChanged,
				current.ValidityRange.ValidFrom,
				previous.Status.ToString(),
				current.Status.ToString());
		}
		
		if (!current.MeasurementNetwork.Equals(previous.MeasurementNetwork)) {
			return new ProjectModification(ProjectModificationType.MeasurementNetworkChanged,
				current.ValidityRange.ValidFrom,
				previous.MeasurementNetwork.Name,
				current.MeasurementNetwork.Name);
		}
		
		
		
		throw new InvalidOperationException("Niet mogelijk om een modificatie op te stellen voor deze twee historiek rijen");
	}
}