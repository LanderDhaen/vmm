namespace Temporal.Domain.Locations;

/// <summary>
///     A geographical Coordinate using the <see cref="ValueObject" /> design pattern.
///     <see cref="https://enterprisecraftsmanship.com/posts/value-objects-explained/" />
/// </summary>
public class Coordinate : ValueObject {
	private Coordinate() {
	}

	public Coordinate(double x, double y) {
		X = x;
		Y = y;
	}

	public double X { get; }

	public double Y { get; }

	protected override IEnumerable<object> GetEqualityComponents() {
		yield return X;
		yield return Y;
	}
}