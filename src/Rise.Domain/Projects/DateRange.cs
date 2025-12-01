namespace Rise.Domain.Projects;

/// <summary>
///     A temporal DateRange using the <see cref="ValueObject" /> design pattern.
///     <see cref="https://enterprisecraftsmanship.com/posts/value-objects-explained/" />
/// </summary>
public class DateRange : ValueObject {
	private DateRange() {
	}

	public DateRange(DateOnly startDate, DateOnly? endDate = null) {
		Guard.Against.OutOfRange(startDate, nameof(startDate), new DateOnly(2000, 1, 1), DateOnly.MaxValue);

		StartDate = startDate;

		if (endDate < startDate) {
			throw new ArgumentException("End date cannot be before start date");
		}

		EndDate = endDate;
	}

	public DateOnly StartDate { get; }

	public DateOnly? EndDate { get; }

	protected override IEnumerable<object> GetEqualityComponents() {
		yield return StartDate;
		yield return EndDate.HasValue ? EndDate.Value : DBNull.Value;
	}
}