// using AngleSharp.Dom;
//
// using Ardalis.Result;
//
// using MudBlazor;
// using MudBlazor.Services;
//
// using NSubstitute;
//
// using Rise.Client.Services;
// using Rise.Client.Stations;
// using Rise.Shared.Common;
// using Rise.Shared.Locations;
// using Rise.Shared.Projects;
// using Rise.Shared.Stations;
//
// internal sealed class ClientTestContext : IDisposable {
// 	public ClientTestContext() {
// 		Ctx.JSInterop.Mode = JSRuntimeMode.Loose;
// 		Ctx.Services.AddMudServices();
// 		Ctx.Services.AddSingleton<BreadcrumbService>();
// 		Ctx.Services.AddSingleton<HeaderService>();
// 	}
//
// 	public TestContext Ctx { get; } = new();
//
// 	public void Dispose() {
// 		Ctx.Dispose();
// 	}
// }
//
// namespace Rise.Client.Tests.Stations {
// 	public class EditShould {
// 		private static void RegisterHappyPathMocks(TestContext ctx) {
// 			IMeasurementStationService? stationSvc = Substitute.For<IMeasurementStationService>();
// 			ILocationService? locationSvc = Substitute.For<ILocationService>();
// 			IProjectService? projectSvc = Substitute.For<IProjectService>();
//
// 			Guid locId = Guid.NewGuid();
// 			LocationResponse.Index locationsIndex = new() {
// 				Locations = new[] {
// 					new LocationDto.Index {
// 						Id = locId,
// 						Name = "Loc A",
// 						Address = new AddressDto.Index {
// 							AddressLine1 = "L1",
// 							AddressLine2 = "L2",
// 							City = "CityA",
// 							PostalCode = "1234",
// 							CountryIso = "BE"
// 						},
// 						Coordinate = new CoordinateDto.Index {
// 							X = 4.35,
// 							Y = 50.85
// 						}
// 					}
// 				}
// 			};
// 			locationSvc.GetIndexAsync(Arg.Any<QueryRequest.SkipTake>(), Arg.Any<CancellationToken>())
// 				.Returns(Task.FromResult(Result.Success(locationsIndex)));
//
// 			Guid projId = Guid.NewGuid();
// 			ProjectResponse.Index projectsIndex = new() {
// 				Projects = new[] {
// 					new ProjectDto.Index {
// 						Id = projId,
// 						Name = "Proj 1",
// 						Code = "P1",
// 						StartDate = new DateOnly(2025, 1, 1),
// 						EndDate = null,
// 						MeasurementNetwork = "Net",
// 						Zone = "Zone",
// 						Status = new Status() // ✅ added required member
// 					}
// 				}
// 			};
// 			projectSvc.GetIndexAsync(Arg.Any<QueryRequest.SkipTake>(), Arg.Any<CancellationToken>())
// 				.Returns(Task.FromResult(Result.Success(projectsIndex)));
//
// 			// Guid stationId = Guid.NewGuid();
// 			// MeasurementStationResponse.Detail detail = new() {
// 			// 	Station = new MeasurementStationDto.Detail {
// 			// 		Id = stationId,
// 			// 		Code = "STN-77",
// 			// 		InstalledAt = new DateTime(2024, 11, 1, 0, 0, 0, DateTimeKind.Utc),
// 			// 		RemovedAt = null,
// 			// 		//Location = "Loc A",
// 			// 		ProjectsCount = 1
// 			// 	}
// 			// };
// 			// stationSvc.GetDetailAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
// 			// 	.Returns(Task.FromResult(Result.Success(detail)));
// 			//
// 			// stationSvc.GetProjectsAsync(Arg.Any<Guid>(), Arg.Any<CancellationToken>())
// 			// 	.Returns(Task.FromResult(Result.Success(new List<ProjectDto.Index>())));
// 			//
// 			// stationSvc.EditAsync(Arg.Any<MeasurementStationRequest.Edit>(), Arg.Any<CancellationToken>())
// 			// 	.Returns(Task.FromResult(Result.Success()));
//
// 			ctx.Services.AddSingleton(stationSvc);
// 			ctx.Services.AddSingleton(locationSvc);
// 			ctx.Services.AddSingleton(projectSvc);
// 		}
//
// 		private static IRenderedFragment RenderWithMudProviders(TestContext ctx) {
// 			return ctx.Render(builder => {
// 				int seq = 0;
// 				builder.OpenComponent<MudPopoverProvider>(seq++);
// 				builder.CloseComponent();
// 				builder.OpenComponent<MudDialogProvider>(seq++);
// 				builder.CloseComponent();
// 				builder.OpenComponent<MudSnackbarProvider>(seq++);
// 				builder.CloseComponent();
// 				builder.OpenComponent<Edit>(seq++);
// 				builder.CloseComponent();
// 			});
// 		}
//
// 		private static IElement FindSaveButton(IRenderedFragment cut) {
// 			return cut.FindAll("button")
// 				.First(b => b.TextContent.Contains("Opslaan", StringComparison.OrdinalIgnoreCase));
// 		}
//
// 		[Fact]
// 		public void Render_SmokeTest() {
// 			using ClientTestContext baseCtx = new();
// 			TestContext ctx = baseCtx.Ctx;
//
// 			RegisterHappyPathMocks(ctx);
//
// 			IRenderedFragment cut = RenderWithMudProviders(ctx);
//
// 			Assert.NotNull(cut);
// 			// Assert.Contains("Bewerk meetstation", cut.Markup);
// 			Assert.Contains("STN-77", cut.Markup);
// 		}
//
// 		[Fact]
// 		public async Task Save_CallsEditAsync() {
// 			using ClientTestContext baseCtx = new();
// 			TestContext ctx = baseCtx.Ctx;
//
// 			RegisterHappyPathMocks(ctx);
// 			IMeasurementStationService stationSvc = ctx.Services.GetRequiredService<IMeasurementStationService>();
//
// 			IRenderedFragment cut = RenderWithMudProviders(ctx);
//
// 			cut.WaitForAssertion(() =>
// 				Assert.Contains("STN-77", cut.Markup)
// 			);
//
// 			await cut.InvokeAsync(() => FindSaveButton(cut).Click());
//
// 			cut.WaitForAssertion(() =>
// 				stationSvc.Received(1).EditAsync(
// 					Arg.Any<MeasurementStationRequest.Edit>(),
// 					Arg.Any<CancellationToken>())
// 			);
// 		}
//
// 		[Fact]
// 		public async Task Save_ShowsError_WhenServiceReturnsNotFound() {
// 			using ClientTestContext baseCtx = new();
// 			TestContext ctx = baseCtx.Ctx;
//
// 			RegisterHappyPathMocks(ctx);
// 			IMeasurementStationService stationSvc = ctx.Services.GetRequiredService<IMeasurementStationService>();
//
// 			stationSvc.EditAsync(Arg.Any<MeasurementStationRequest.Edit>(), Arg.Any<CancellationToken>())
// 				.Returns(Task.FromResult(Result.NotFound()));
//
// 			IRenderedFragment cut = RenderWithMudProviders(ctx);
//
// 			cut.WaitForAssertion(() =>
// 				Assert.Contains("STN-77", cut.Markup)
// 			);
//
// 			await cut.InvokeAsync(() => FindSaveButton(cut).Click());
//
// 			cut.WaitForAssertion(() =>
// 				stationSvc.Received(1).EditAsync(
// 					Arg.Any<MeasurementStationRequest.Edit>(),
// 					Arg.Any<CancellationToken>())
// 			);
// 		}
// 	}
// }