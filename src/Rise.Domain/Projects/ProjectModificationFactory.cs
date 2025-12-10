namespace Rise.Domain.Projects;
/// <summary>
///     Factory responsible for creating <see cref="ProjectModification"/> objects
///     from a pair of <see cref="ProjectHistory"/> entries.
/// </summary>
public static class ProjectModificationFactory {
	public static List<ProjectModification> Create(ProjectHistory current, ProjectHistory previous) {
		var modifications = new List<ProjectModification>();
		
		if (!current.Status.Equals(previous.Status)) {
			modifications.Add(new ProjectModification(ProjectModificationType.StatusChanged,
				current.ValidityRange.ValidFrom,
				previous.Status.ToString(),
				current.Status.ToString()));
		}
		
		if (!current.DateRange.StartDate.Equals(previous.DateRange.StartDate)) {
			modifications.Add(new ProjectModification(ProjectModificationType.StartDateChanged,
				current.ValidityRange.ValidFrom,
				previous.DateRange.StartDate.ToString(),
				current.DateRange.StartDate.ToString()));
		}

		if (!current.DateRange.EndDate.Equals(previous.DateRange.EndDate)) {
			modifications.Add(new ProjectModification(ProjectModificationType.EndDateChanged,
				current.ValidityRange.ValidFrom,
				previous.DateRange.EndDate?.ToString() ?? "-",
				current.DateRange.EndDate?.ToString() ?? "-"));
		}

		if (!current.MeasurementNetwork.Equals(previous.MeasurementNetwork)) {
			modifications.Add(new ProjectModification(ProjectModificationType.MeasurementNetworkChanged,
				current.ValidityRange.ValidFrom,
				previous.MeasurementNetwork.Name,
				current.MeasurementNetwork.Name));
		}

		if (modifications.Count == 0) {
			throw new InvalidOperationException(
				"Niet mogelijk om de modificaties op te stellen voor deze twee historiek rijen");
		}
		
		return modifications;
	}
}