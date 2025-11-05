namespace Temporal.Domain.Common;

/// <summary>
///     Entity Base Class, all entities should inherit from this. (read: Entity = Row in SQL terms)
/// </summary>
public abstract class Entity {
	protected Entity() { }

	protected Entity(Guid id) {
		Id = id;
	}

	/// <summary>
	///     Primary Key of the <see cref="Entity" />
	/// </summary>
	public Guid Id { get; protected set; }

	/// <summary>
	///     Date of the initial creation.
	/// </summary>
	public DateTime CreatedAt { get; set; }

	/// <summary>
	///     Date of the last update.
	/// </summary>
	public DateTime UpdatedAt { get; set; }

	/// <summary>
	///     Soft Delete indicator, instead of deleting rows, we flag them as deleted.
	/// </summary>
	public bool IsDeleted { get; set; }

	public override bool Equals(object? obj) {
		if (obj is not Entity other) {
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

	public static bool operator ==(Entity a, Entity b) {
		if (a is null && b is null) {
			return true;
		}

		if (a is null || b is null) {
			return false;
		}

		return a.Equals(b);
	}

	public static bool operator !=(Entity a, Entity b) {
		return !(a == b);
	}

	public override int GetHashCode() {
		return (GetType().ToString() + Id).GetHashCode();
	}
}