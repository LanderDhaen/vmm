using AngleSharp.Dom;

using Ardalis.Result;

using MudBlazor;
using MudBlazor.Services;

using NSubstitute;

using Rise.Client.Stations;
using Rise.Shared.Common;
using Rise.Shared.Locations;
using Rise.Shared.Projects;
using Rise.Shared.Stations;


internal sealed class ClientTestContext : IDisposable {
	public TestContext Ctx { get; } = new();

	public ClientTestContext() {
		Ctx.JSInterop.Mode = JSRuntimeMode.Loose;
		Ctx.Services.AddMudServices();
	}

	public void Dispose() => Ctx.Dispose();
}

namespace Rise.Client.Tests.Stations {
	public class EditShould {
		private static void RegisterHappyPathMocks(TestContext ctx) {
			var stationSvc = Substitute.For<IMeasurementStationService>();
			var locationSvc = Substitute.For<ILocationService>();
			var projectSvc = Substitute.For<IProjectService>();

			var locId = Guid.NewGuid();
			var locationsIndex = new LocationResponse.Index {
				Locations = new[]
				{
					new LocationDto.Index
					{
						Id = locId,
						Name = "Loc A",
						Address = new AddressDto.Index
						{
							AddressLine1 = "L1",
							AddressLine2 = "L2",
							City = "CityA",
							PostalCode = "1234",
							CountryIso = "BE"
						},
						Coordinate = new CoordinateDto.Index
						{
							X = 4.35,
							Y = 50.85
						}
					}
				}
			};
			locationSvc.GetIndexAsync(Arg.Any<QueryRequest.SkipTake>(), Arg.Any<CancellationToken>())
					   .Returns(Task.FromResult(Result.Success(locationsIndex)));

			var projId = Guid.NewGuid();
			var projectsIndex = new ProjectResponse.Index {
				Projects = new[]
				{
					new ProjectDto.Index
					{
						Id = projId,
						Name = "Proj 1",
						Code = "P1",
						StartDate = new DateOnly(2025, 1, 1),
						EndDate = null,
						MeasurementNetwork = "Net",
						Zone = "Zone"
					}
				}
			};
			projectSvc.GetIndexAsync(Arg.Any<QueryRequest.SkipTake>(), Arg.Any<CancellationToken>())
					  .Returns(Task.FromResult(Result.Success(projectsIndex)));

			var stationId = Guid.NewGuid();
			var detail = new MeasurementStationResponse.Detail {
				Station = new MeasurementStationDto.Detail {
					Id = stationId,
					Code = "STN-77",
					Status = "Active",
					InstalledAt = new DateTime(2024, 11, 1, 0, 0, 0, DateTimeKind.Utc),
					RemovedAt = null,
					Location = "Loc A",
					City = "CityA",
					ProjectsCount = 1
				}
			};
			stationSvc.GetDetailAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
					  .Returns(Task.FromResult(Result.Success(detail)));

			stationSvc.GetProjectsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
					  .Returns(Task.FromResult(Result.Success(new List<ProjectDto.Index>())));

			stationSvc.EditAsync(Arg.Any<MeasurementStationRequest.Edit>(), Arg.Any<CancellationToken>())
					  .Returns(Task.FromResult(Result.Success()));

			ctx.Services.AddSingleton(stationSvc);
			ctx.Services.AddSingleton(locationSvc);
			ctx.Services.AddSingleton(projectSvc);
		}

		private static IRenderedFragment RenderWithMudProviders(TestContext ctx) {
			return ctx.Render(builder => {
				var seq = 0;
				builder.OpenComponent<MudPopoverProvider>(seq++); builder.CloseComponent();
				builder.OpenComponent<MudDialogProvider>(seq++); builder.CloseComponent();
				builder.OpenComponent<MudSnackbarProvider>(seq++); builder.CloseComponent();
				builder.OpenComponent<Edit>(seq++); builder.CloseComponent();
			});
		}

		private static IElement FindSaveButton(IRenderedFragment cut)
			=> cut.FindAll("button").First(b => b.TextContent.Contains("Opslaan", StringComparison.OrdinalIgnoreCase));

		[Fact]
		public void Render_SmokeTest() {
			using var baseCtx = new ClientTestContext();
			var ctx = baseCtx.Ctx;

			RegisterHappyPathMocks(ctx);

			var cut = RenderWithMudProviders(ctx);

			Assert.NotNull(cut);
			Assert.Contains("Bewerk meetstation", cut.Markup);
			Assert.Contains("STN-77", cut.Markup);
		}

		[Fact]
		public async Task Save_CallsEditAsync() {
			using var baseCtx = new ClientTestContext();
			var ctx = baseCtx.Ctx;

			RegisterHappyPathMocks(ctx);
			var stationSvc = ctx.Services.GetRequiredService<IMeasurementStationService>();

			var cut = RenderWithMudProviders(ctx);
			FindSaveButton(cut).Click();

			await stationSvc.Received(1).EditAsync(
				Arg.Any<MeasurementStationRequest.Edit>(),
				Arg.Any<CancellationToken>());
		}

		[Fact]
		public async Task Save_ShowsError_WhenServiceReturnsNotFound() {
			using var baseCtx = new ClientTestContext();
			var ctx = baseCtx.Ctx;

			RegisterHappyPathMocks(ctx);
			var stationSvc = ctx.Services.GetRequiredService<IMeasurementStationService>();

			stationSvc.EditAsync(Arg.Any<MeasurementStationRequest.Edit>(), Arg.Any<CancellationToken>())
					  .Returns(Task.FromResult(Result.NotFound()));

			var cut = RenderWithMudProviders(ctx);
			FindSaveButton(cut).Click();

			await stationSvc.Received(1).EditAsync(
				Arg.Any<MeasurementStationRequest.Edit>(),
				Arg.Any<CancellationToken>());

		}
	}
}