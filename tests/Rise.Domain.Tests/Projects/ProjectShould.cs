using Rise.Domain.Projects;

namespace Rise.Domain.Tests.Projects;

public class ProjectShould {
	[Fact]
	public void BeCreated() {
		Address address = new("Teststraat", "Testbus 1", "Teststad", "9300");
		Project project = new("Testing Project", "CODE123", new DateTime(2025, 1, 5), new DateTime(2025, 5, 10),
			address);

		project.Name.ShouldBe("Testing Project");
		project.Code.ShouldBe("CODE123");
		project.StartDate.ShouldBe(new DateTime(2025, 1, 5));
		project.EndDate.ShouldBe(new DateTime(2025, 5, 10));
		project.Location.ShouldBe(address);
	}

	[Theory]
	[InlineData(null)]
	[InlineData("   ")]
	[InlineData("")]
	public void NotBeCreatedWithAnInvalidName(string? name) {
		Action act = () => {
			Address address = new("Teststraat", "Testbus 1", "Teststad", "9300");
			Project project = new(name!, "CODE123", new DateTime(2025, 1, 5), new DateTime(2025, 5, 10), address);
		};
		act.ShouldThrow<ArgumentException>();
	}

	[Theory]
	[InlineData(null)]
	[InlineData("   ")]
	[InlineData("")]
	public void NotBeCreatedWithAnInvalidCode(string? code) {
		Action act = () => {
			Address address = new("Teststraat", "Testbus 1", "Teststad", "9300");
			Project project = new("Testproject", code!, new DateTime(2025, 1, 5), new DateTime(2025, 5, 10), address);
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
			Address address = new("Teststraat", "Testbus 1", "Teststad", "9300");
			Project project = new("Testproject", "CODE123", new DateTime(year, month, day), new DateTime(2025, 5, 10),
				address);
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
			Address address = new("Teststraat", "Testbus 1", "Teststad", "9300");
			Project project = new("Testproject", "CODE123", new DateTime(2025, 1, 5), new DateTime(year, month, day),
				address);
		};
		act.ShouldThrow<ArgumentException>();
	}

	[Theory]
	[InlineData(null)]
	public void NotBeCreatedWithAnInvalidAddress(Address? address) {
		Action act = () => {
			Project project = new("Testproject", "CODE123", new DateTime(2025, 1, 5), new DateTime(2025, 5, 10),
				address!);
		};
		act.ShouldThrow<ArgumentException>();
	}
}