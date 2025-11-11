// using AngleSharp.Dom;
//
// using Ardalis.Result;
//
// using MudBlazor;
//
// using NSubstitute;
//
// using Rise.Shared.Projects;
//
// using Shouldly;
//
// using Xunit.Abstractions;
//
// namespace Rise.Client.Projects;
//
// public class DetailShould : TestContext {
// 	public DetailShould(ITestOutputHelper outputHelper) {
// 		Services.AddXunitLogger(outputHelper);
// 		this.AddTestAuthorization().SetAuthorized("TEST USER");
// 		Services.AddSingleton(Substitute.For<IBrowserViewportService>());
// 	}
//
// 	[Fact]
// 	public void ShowsProjectDetails_WhenProjectExists() {
// 		// Arrange
// 		Guid projectId = Guid.NewGuid();
// 		ProjectDto.Detail project = new() {
// 			Id = projectId,
// 			Name = "Watermetingen",
// 			Code = "WTR-01",
// 			StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
// 			EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(7)),
// 			MeasurementNetwork = "Meetnet West",
// 			Zone = "Gent"
// 		};
//
//
// 		ProjectResponse.Detail response = new() { Project = project };
//
// 		IProjectService? projectServiceMock = Substitute.For<IProjectService>();
// 		projectServiceMock.GetDetailAsync(projectId).Returns(Task.FromResult(Result.Success(response)));
//
// 		Services.AddScoped(_ => projectServiceMock);
//
// 		// Act
// 		IRenderedComponent<Detail> cut =
// 			RenderComponent<Detail>(parameters => parameters.Add(p => p.ProjectId, projectId));
//
// 		// Assert
// 		// Check if project name renders
// 		cut.Find("div.d-flex.align-center.m-2 > .mud-typography").TextContent.ShouldBe("Watermetingen");
//
// 		// Check if code and network render
// 		IRefreshableElementCollection<IElement> papers = cut.FindAll("div.mud-item > .mud-paper");
// 		papers.Count.ShouldBeGreaterThanOrEqualTo(6); // There are at least 6 fields
//
// 		papers[0].TextContent.ShouldBe("WTR-01"); // Code
// 		papers[1].TextContent.ShouldBe("Watermetingen"); // Name
// 		papers[4].TextContent.ShouldBe("Meetnet West"); // MeasurementNetwork
// 		papers[5].TextContent.ShouldBe("Gent"); // Zone
// 	}
// }

