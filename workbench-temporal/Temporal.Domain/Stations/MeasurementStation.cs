using Temporal.Domain.Locations;

namespace Temporal.Domain.Stations;

public class MeasurementStation : Entity {
	public string Code { get; private set; } = default!;
	public Location Location { get; private set; } = default!;
	public Installation Installation { get; init; } = default!;

	private readonly List<MeasurementStationHistory> _measurementStationHistories = [];
	public IReadOnlyList<MeasurementStationHistory> MeasurementStationHistories => _measurementStationHistories.AsReadOnly();
	/// <summary>
	///     Entity Framework Core Constructor
	/// </summary>
	private MeasurementStation() { }

	public MeasurementStation(string code, Location location, Installation installation) {
		Code = Guard.Against.NullOrWhiteSpace(code, nameof(code));
		Location = Guard.Against.Null(location, nameof(location));
		Installation = Guard.Against.Null(installation, nameof(installation));
	}

	public void Edit(string code) {

		// Validate the input before creating the history & assigning the values

		Guard.Against.NullOrWhiteSpace(code, nameof(code));

		// Create history object using the old values

		ValidityRange validityRange = new(UpdatedAt, DateTime.Now);
		MeasurementStationHistory measurementStationHistory = new(Code, Location, new Installation(Installation.InstalledAt, Installation.RemovedAt), validityRange);
		_measurementStationHistories.Add(measurementStationHistory);

		// Assign the new values

		Code = code;
	}
}


public class MeasurementStationHistory : HistoryEntity {
	public string Code { get; private set; }

	public Location Location { get; private set; }

	public Installation Installation { get; init; }

	/// <summary>
	///     Entity Framework Core Constructor
	/// </summary>
	private MeasurementStationHistory() {
	}
	public MeasurementStationHistory(string code, Location location, Installation installation, ValidityRange validityRange) {
		Code = Guard.Against.NullOrWhiteSpace(code, nameof(code));
		Location = Guard.Against.Null(location, nameof(location));
		Installation = Guard.Against.Null(installation, nameof(installation));
		ValidityRange = Guard.Against.Null(validityRange, nameof(validityRange));
	}

	public override string ToString() {
		return $"{Code} " + base.ToString();
	}
}