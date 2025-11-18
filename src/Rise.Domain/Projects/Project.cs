using Rise.Domain.Networks;
using Rise.Domain.Stations;

namespace Rise.Domain.Projects;

/// <summary>
///     A project is something that owns multiple measuring stations.
/// </summary>
public class Project : Entity {
	private string _code = string.Empty;
	private DateRange _dateRange;
	private MeasurementNetwork _measurementNetwork;
	private string _name = string.Empty;

	private Project() { }

	public Project(string name, string code, DateRange dateRange, MeasurementNetwork measurementNetwork) {
		Name = name;
		Code = code;
		DateRange = Guard.Against.Null(dateRange);
		MeasurementNetwork = Guard.Against.Null(measurementNetwork);
		UpdateStatusBasedOnEndDate();
	}

	public string Name {
		get => _name;
		private set => _name = Guard.Against.NullOrWhiteSpace(value);
	}

	public string Code {
		get => _code;
		private set => _code = Guard.Against.NullOrWhiteSpace(value);
	}

	public DateRange DateRange {
		get => _dateRange;
		private set => _dateRange = Guard.Against.Null(value);
	}

	public ICollection<MeasurementStation> Stations { get; private set; } =
		new List<MeasurementStation>();

	public MeasurementNetwork MeasurementNetwork {
		get => _measurementNetwork;
		private set => _measurementNetwork = Guard.Against.Null(value);
	}

	public Status Status { get; private set; } = Status.Actief;

	public void Edit(string name, string code, DateRange dateRange, MeasurementNetwork measurementNetwork) {
		if (Status == Status.Afgesloten) {
			throw new InvalidOperationException("Afgesloten projecten kunnen niet worden bewerkt.");
		}

		Name = name;
		Code = code;
		DateRange = dateRange;
		MeasurementNetwork = measurementNetwork;

		UpdateStatusBasedOnEndDate();
	}

	public void UpdateStatus(Status newStatus) {
		if (Status == newStatus) {
			return;
		}

		DateRange = newStatus switch {
			Status.Actief => new DateRange(DateRange.StartDate),
			Status.Afgesloten when !DateRange.EndDate.HasValue => new DateRange(DateRange.StartDate,
				DateOnly.FromDateTime(DateTime.UtcNow)),
			_ => DateRange
		};

		Status = newStatus;
	}

	private void UpdateStatusBasedOnEndDate() {
		Status = DateRange.EndDate.HasValue
			? DateRange.EndDate.Value <= DateOnly.FromDateTime(DateTime.UtcNow) ? Status.Afgesloten : Status.Actief
			: Status.Actief;
	}
}