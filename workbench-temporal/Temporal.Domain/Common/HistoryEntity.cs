namespace Temporal.Domain.Common;

/// <summary>
///     History Entity Base Class, all history entities should inherit from this. (read: History Entity = Row in history table in SQL terms)
/// </summary>
public abstract class HistoryEntity {
	protected HistoryEntity() { }

	protected HistoryEntity(Guid id) {
		Id = id;
	}

	/// <summary>
	///     Primary Key of the <see cref="HistoryEntity" />
	/// </summary>
	public Guid Id { get; protected set; }

	public ValidityRange ValidityRange { get; init; }

	public override bool Equals(object? obj) {
		if (obj is not HistoryEntity other) {
			return false;
		}

		if (ReferenceEquals(this, other)) {
			return true;
		}

		if (GetType() != other.GetType()) {
			return false;
		}

		if (Id.Equals(default) || other.Id.Equals(default)) {
			return false;
		}

		return Id.Equals(other.Id);
	}

	public static bool operator ==(HistoryEntity a, HistoryEntity b) {
		if (a is null && b is null) {
			return true;
		}

		if (a is null || b is null) {
			return false;
		}

		return a.Equals(b);
	}

	public static bool operator !=(HistoryEntity a, HistoryEntity b) {
		return !(a == b);
	}

	public override int GetHashCode() {
		return (GetType().ToString() + Id).GetHashCode();
	}

	public override string ToString() {
		return $"({ValidityRange.ValidFrom} - {ValidityRange.ValidTo})";
	}
}