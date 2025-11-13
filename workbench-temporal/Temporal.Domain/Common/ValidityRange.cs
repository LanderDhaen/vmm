namespace Temporal.Domain.Common;

/// <summary>
///     A temporal ValidityRange using the <see cref="ValueObject" /> design pattern.
///     <see cref="https://enterprisecraftsmanship.com/posts/value-objects-explained/" />
/// </summary>
public class ValidityRange : ValueObject {
	private ValidityRange() {

	}

	public ValidityRange(DateTime validFrom, DateTime validTo) {

		if (validTo < validFrom) {
			throw new ArgumentException("End date cannot be before start date");
		}

		ValidFrom = validFrom;
		ValidTo = validTo;
	}

	public DateTime ValidFrom { get; init; } = default!;

	public DateTime ValidTo { get; init; } = default!;

	protected override IEnumerable<object> GetEqualityComponents() {
		yield return ValidFrom;
		yield return ValidTo;
	}
}