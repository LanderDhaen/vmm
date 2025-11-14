namespace Temporal.Domain.Stations;

public sealed class Installation : ValueObject {
	private Installation() { }

	public Installation(DateTime installedAt, DateTime? removedAt = null) {
		if (removedAt is not null && removedAt < installedAt) {
			throw new ArgumentException("RemovedAt cannot be earlier than InstalledAt.", nameof(removedAt));
		}

		InstalledAt = installedAt;
		RemovedAt = removedAt;
	}

	public DateTime InstalledAt { get; }
	public DateTime? RemovedAt { get; }

	public Installation MarkRemoved(DateTime removedAt) {
		return new Installation(InstalledAt, removedAt);
	}

	protected override IEnumerable<object> GetEqualityComponents() {
		yield return InstalledAt;
		yield return RemovedAt ?? default(DateTime?);
	}
}