using Temporal.Domain.Locations;

namespace Temporal.Domain.Stations;

public class MeasurementStation : Entity {
	private readonly List<MeasurementStationHistory> _measurementStationHistories = [];

	/// <summary>
	///     Entity Framework Core Constructor
	/// </summary>
	private MeasurementStation() { }

	public MeasurementStation(string code, Location location, Installation installation) {
		Code = Guard.Against.NullOrWhiteSpace(code);
		Location = Guard.Against.Null(location);
		Installation = Guard.Against.Null(installation);
	}

	public string Code { get; private set; } = default!;
	public Location Location { get; } = default!;
	public Installation Installation { get; init; } = default!;

	public IReadOnlyList<MeasurementStationHistory> MeasurementStationHistories =>
		_measurementStationHistories.AsReadOnly();

	public void Edit(string code) {
		// Validate the input before creating the history & assigning the values

		Guard.Against.NullOrWhiteSpace(code);

		// Create history object using the old values

		ValidityRange validityRange = new(UpdatedAt, DateTime.Now);
		MeasurementStationHistory measurementStationHistory = new(Code, Location,
			new Installation(Installation.InstalledAt, Installation.RemovedAt), validityRange);
		_measurementStationHistories.Add(measurementStationHistory);

		// Assign the new values

		Code = code;
	}
}

public class MeasurementStationHistory : HistoryEntity {
	/// <summary>
	///     Entity Framework Core Constructor
	/// </summary>
	private MeasurementStationHistory() {
	}

	public MeasurementStationHistory(string code, Location location, Installation installation,
		ValidityRange validityRange) {
		Code = Guard.Against.NullOrWhiteSpace(code);
		Location = Guard.Against.Null(location);
		Installation = Guard.Against.Null(installation);
		ValidityRange = Guard.Against.Null(validityRange);
	}

	public string Code { get; }

	public Location Location { get; private set; }

	public Installation Installation { get; init; }

	public override string ToString() {
		return $"{Code} " + base.ToString();
	}
}