using Rise.Domain.Projects;
using Rise.Domain.Locations;

namespace Rise.Domain.Tests.Projects;

public class ProjectShould {
	[Fact]
	public void BeCreated() {
		Location location = new("Aalst", new Address("Koningstraat 12", "Bus 3A", "Brussel", "1000", "BE"),
			new Coordinate(6.0, 7.0));
		Project project = new("Testing Project", "CODE123", new DateTime(2025, 1, 5), new DateTime(2025, 5, 10),
			location);

		project.Name.ShouldBe("Testing Project");
		project.Code.ShouldBe("CODE123");
		project.StartDate.ShouldBe(new DateTime(2025, 1, 5));
		project.EndDate.ShouldBe(new DateTime(2025, 5, 10));
		project.Location.ShouldBe(location);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("   ")]
	[InlineData("")]
	public void NotBeCreatedWithAnInvalidName(string? name) {
		Action act = () => {
			Location location = new("Aalst", new Address("Koningstraat 12", "Bus 3A", "Brussel", "1000", "BE"),
				new Coordinate(6.0, 7.0));
			Project project = new(name!, "CODE123", new DateTime(2025, 1, 5), new DateTime(2025, 5, 10), location);
		};
		act.ShouldThrow<ArgumentException>();
	}

	[Theory]
	[InlineData(null)]
	[InlineData("   ")]
	[InlineData("")]
	public void NotBeCreatedWithAnInvalidCode(string? code) {
		Action act = () => {
			Location location = new("Aalst", new Address("Koningstraat 12", "Bus 3A", "Brussel", "1000", "BE"),
				new Coordinate(6.0, 7.0));
			Project project = new("Testproject", code!, new DateTime(2025, 1, 5), new DateTime(2025, 5, 10), location);
		};
		act.ShouldThrow<ArgumentException>();
	}

	[Theory]
	[InlineData(0, 0, 0)]
	[InlineData(1, 1, 1)]
	[InlineData(10000, 1, 1)]
	[InlineData(999999, 1, 1)]
	public void NotBeCreatedWithAnInvalidStartDate(int year, int month, int day) {
		Action act = () => {
			Location location = new("Aalst", new Address("Koningstraat 12", "Bus 3A", "Brussel", "1000", "BE"),
				new Coordinate(6.0, 7.0));
			Project project = new("Testproject", "CODE123", new DateTime(year, month, day), new DateTime(2025, 5, 10),
				location);
		};
		act.ShouldThrow<ArgumentException>();
	}

	[Theory]
	[InlineData(0, 0, 0)]
	[InlineData(1, 1, 1)]
	[InlineData(10000, 1, 1)]
	[InlineData(999999, 1, 1)]
	public void NotBeCreatedWithAnInvalidEndDate(int year, int month, int day) {
		Action act = () => {
			Location location = new("Aalst", new Address("Koningstraat 12", "Bus 3A", "Brussel", "1000", "BE"),
				new Coordinate(6.0, 7.0));
			Project project = new("Testproject", "CODE123", new DateTime(2025, 1, 5), new DateTime(year, month, day),
				location);
		};
		act.ShouldThrow<ArgumentException>();
	}

	[Theory]
	[InlineData(null)]
	public void NotBeCreatedWithAnInvalidAddress(Location? location) {
		Action act = () => {
			Project project = new("Testproject", "CODE123", new DateTime(2025, 1, 5), new DateTime(2025, 5, 10),
				location!);
		};
		act.ShouldThrow<ArgumentException>();
	}
}