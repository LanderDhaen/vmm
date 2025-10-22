namespace Rise.Domain.Locations;

/// <summary>
///     A physical Coördinate using the <see cref="ValueObject" /> design pattern.
///     <see cref="https://enterprisecraftsmanship.com/posts/value-objects-explained/" />
/// </summary>
public class Coordinate : ValueObject {
	private Coordinate() {
		
	}

	public Coordinate(double x, double y) {
		x = x;
		y = y;
	}

	public double X { get; } = default!;

	public double Y { get; } = default!;

	protected override IEnumerable<object> GetEqualityComponents() {
		yield return X;
		yield return Y;
	}
}