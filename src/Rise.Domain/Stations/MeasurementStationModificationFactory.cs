namespace Rise.Domain.Stations
{
    /// <summary>
    ///     Factory responsible for creating <see cref="MeasurementStationModification"/> objects
    ///     from a pair of <see cref="MeasurementStationHistory"/> entries.
    /// </summary>
    public static class MeasurementStationModificationFactory {
        public static MeasurementStationModification Create(
            MeasurementStationHistory current,
            MeasurementStationHistory previous)
        {
            if (!current.Location.Equals(previous.Location)) {
                return new MeasurementStationModification(
                    MeasurementStationModificationType.LocationChanged,
                    current.ValidityRange.ValidFrom,
                    previous.Location.Name,
                    current.Location.Name
                );
            }

            if (!current.Installation.InstalledAt.Equals(previous.Installation.InstalledAt)) {
                return new MeasurementStationModification(
                    MeasurementStationModificationType.InstalledAtChanged,
                    current.ValidityRange.ValidFrom,
                    previous.Installation.InstalledAt.ToString(),
                    current.Installation.InstalledAt.ToString()
                );
            }

            if (!current.Installation.RemovedAt.Equals(previous.Installation.RemovedAt)) {
                return new MeasurementStationModification(
                    MeasurementStationModificationType.RemovedAtChanged,
                    current.ValidityRange.ValidFrom,
                    previous.Installation.RemovedAt?.ToString() ?? "-",
                    current.Installation.RemovedAt?.ToString() ?? "-"
                );
            }

            throw new InvalidOperationException("Can't create a modification for these history entries");
        }
    }
}
