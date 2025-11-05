namespace Temporal.Domain.Stations;

public sealed class Installation : ValueObject {
	public DateTime InstalledAt { get; private set; }
	public DateTime? RemovedAt { get; private set; }

	private Installation() { }

	public Installation(DateTime installedAt, DateTime? removedAt = null) {
		if (removedAt is not null && removedAt < installedAt)
			throw new ArgumentException("RemovedAt cannot be earlier than InstalledAt.", nameof(removedAt));

		InstalledAt = installedAt;
		RemovedAt = removedAt;
	}

	public Installation MarkRemoved(DateTime removedAt)
		=> new(InstalledAt, removedAt);

	protected override IEnumerable<object> GetEqualityComponents() {
		yield return InstalledAt;
		yield return RemovedAt ?? default(DateTime?);
	}
}