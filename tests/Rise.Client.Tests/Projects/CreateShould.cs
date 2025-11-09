using MudBlazor;
using MudBlazor.Services;

using NSubstitute;

using Rise.Client.Networks;
using Rise.Shared.Common;
using Rise.Shared.Networks;
using Rise.Shared.Projects;

using Shouldly;

using Xunit.Abstractions;

namespace Rise.Client.Projects;

public class CreateShould : TestContext {
	public CreateShould(ITestOutputHelper outputHelper) {
		Services.AddXunitLogger(outputHelper);
		Services.AddScoped<INetworkService, FakeNetworkService>();
		Services.AddMudServices();
		// Avoid strict JS interop errors for MudBlazor components
		JSInterop.Mode = JSRuntimeMode.Loose;
	}

	[Fact]
	public void ShouldShowCreateProjectForm() {
		// Authenticate as a test user for this specific test.
		//this.AddTestAuthorization().SetAuthorized("TEST USER");

		// Needed mock, even if not used in this test
		var projectServiceMock = Substitute.For<IProjectService>();
		Services.AddScoped(provider => projectServiceMock);

		var modelMock = Substitute.For<ProjectRequest.Create.Validator>();
		Services.AddScoped(provider => modelMock);

		RenderComponent<MudPopoverProvider>();
		IRenderedComponent<Create> cut = RenderComponent<Create>();
		var textFields = cut.FindComponents<MudTextField<string>>();

		textFields.Any(tf => tf.Instance.Placeholder == "Naam").ShouldBeTrue();
		textFields.Any(tf => tf.Instance.Placeholder == "Referentienummer").ShouldBeTrue();

		cut.FindComponent<MudDatePicker>().ShouldNotBeNull();

		cut.FindComponent<MudAutocomplete<Guid?>>().ShouldNotBeNull();
	}

	[Fact]
	public void ShouldShowError_WithNameFieldEmpty() {
		var projectServiceMock = Substitute.For<IProjectService>();
		Services.AddScoped(provider => projectServiceMock);

		RenderComponent<MudPopoverProvider>();
		IRenderedComponent<Create> cut = RenderComponent<Create>();
		cut.FindComponents<MudTextField<string>>()
			.First(tf => tf.Instance.Placeholder == "Referentienummer")
			.SetParametersAndRender(p => p.Add(c => c.Value, "PRJ-001"));

		cut.FindComponent<MudDatePicker>()
			.SetParametersAndRender(p => p.Add(c => c.Date, DateTime.Today));

		var firstNetwork = (cut.Instance.NetworkService.GetIndexAsync(new QueryRequest.SkipTake()))
			.Result.Value.measurementsNetworks.First();
		
		cut.FindComponent<MudAutocomplete<Guid?>>()
			.SetParametersAndRender(p => p.Add(c => c.Value, firstNetwork.Id));
		
		//cut.FindComponent<MudForm>().Instance.Validate();
		//cut.FindComponent<MudButton>().Click();
		var button = cut.FindAll("button")
			.First(b => b.TextContent.Contains("Opslaan"));
		
		// https://cgi-fr.github.io/IoT-Hub-Portal/dev/dev-guide/testing/unit-tests-on-blazor-components/#how-to-unit-test-a-component-requiring-an-external-component
		
		//cut.Render();
		cut.FindAll("div")
			.Any(d => d.TextContent.Contains("Naam mag niet leeg zijn."))
			.ShouldBeTrue();
		
		cut.FindAll("div")
			.Any(d => d.TextContent.Contains("Code mag niet leeg zijn."))
			.ShouldBeFalse();
		
		cut.FindAll("div")
			.Any(d => d.TextContent.Contains("Startdatum mag niet leeg zijn."))
			.ShouldBeFalse();
		
		cut.FindAll("div")
			.Any(d => d.TextContent.Contains("Je moet een meetnet selecteren."))
			.ShouldBeFalse();
	}

	[Fact]
	public void ShouldShowError_WithCodeFieldEmpty() {
		var projectServiceMock = Substitute.For<IProjectService>();
		Services.AddScoped(provider => projectServiceMock);

		RenderComponent<MudPopoverProvider>();
		IRenderedComponent<Create> cut = RenderComponent<Create>();
		cut.FindComponents<MudTextField<string>>()
			.First(tf => tf.Instance.Placeholder == "Naam")
			.SetParametersAndRender(p => p.Add(c => c.Value, "Test Project"));

		cut.FindComponent<MudDatePicker>()
			.SetParametersAndRender(p => p.Add(c => c.Date, DateTime.Today));

		var firstNetwork = (cut.Instance.NetworkService.GetIndexAsync(new QueryRequest.SkipTake()))
			.Result.Value.measurementsNetworks.First();
		
		cut.FindComponent<MudAutocomplete<Guid?>>()
			.SetParametersAndRender(p => p.Add(c => c.Value, firstNetwork.Id));
		
		cut.FindComponent<MudForm>().Instance.Validate();

		cut.Render();
		
		// Test passes but not really sure if it's correct
		cut.FindAll("div")
			.Any(d => d.TextContent.Contains("Code mag niet leeg zijn."))
			.ShouldBeTrue();
		
		cut.FindAll("div")
			.Any(d => d.TextContent.Contains("Naam mag niet leeg zijn."))
			.ShouldBeFalse();
		
		cut.FindAll("div")
			.Any(d => d.TextContent.Contains("Startdatum mag niet leeg zijn."))
			.ShouldBeFalse();
		
		cut.FindAll("div")
			.Any(d => d.TextContent.Contains("Je moet een meetnet selecteren."))
			.ShouldBeFalse();
	}

	[Fact]
	public void ShouldShowError_WithStartDateFieldEmpty() {
		var projectServiceMock = Substitute.For<IProjectService>();
		Services.AddScoped(provider => projectServiceMock);

		RenderComponent<MudPopoverProvider>();
		IRenderedComponent<Create> cut = RenderComponent<Create>();
		cut.FindComponents<MudTextField<string>>()
			.First(tf => tf.Instance.Placeholder == "Code")
			.SetParametersAndRender(p => p.Add(c => c.Value, "PRJ-001"));
		
		cut.FindComponents<MudTextField<string>>()
			.First(tf => tf.Instance.Placeholder == "Naam")
			.SetParametersAndRender(p => p.Add(c => c.Value, "Test Project"));

		cut.FindComponent<MudDatePicker>()
			.SetParametersAndRender(p => p.Add(c => c.Date, null));

		var firstNetwork = (cut.Instance.NetworkService.GetIndexAsync(new QueryRequest.SkipTake()))
			.Result.Value.measurementsNetworks.First();
		
		cut.FindComponent<MudAutocomplete<Guid?>>()
			.SetParametersAndRender(p => p.Add(c => c.Value, firstNetwork.Id));
		
		cut.FindComponent<MudForm>().Instance.Validate();

		cut.Render();
		
		cut.FindAll("div")
			.Any(d => d.TextContent.Contains("Startdatum mag niet leeg zijn."))
			.ShouldBeTrue();
		
		cut.FindAll("div")
			.Any(d => d.TextContent.Contains("Code mag niet leeg zijn."))
			.ShouldBeFalse();
		
		cut.FindAll("div")
			.Any(d => d.TextContent.Contains("Naam mag niet leeg zijn."))
			.ShouldBeFalse();
		
		cut.FindAll("div")
			.Any(d => d.TextContent.Contains("Je moet een meetnet selecteren."))
			.ShouldBeFalse();
	}

	[Fact]
	public void ShouldShowError_WithMeasurementNetworkNotSelected() {
		var projectServiceMock = Substitute.For<IProjectService>();
		Services.AddScoped(provider => projectServiceMock);

		RenderComponent<MudPopoverProvider>();
		IRenderedComponent<Create> cut = RenderComponent<Create>();
		cut.FindComponents<MudTextField<string>>()
			.First(tf => tf.Instance.Placeholder == "Code")
			.SetParametersAndRender(p => p.Add(c => c.Value, "PRJ-001"));
		
		cut.FindComponents<MudTextField<string>>()
			.First(tf => tf.Instance.Placeholder == "Naam")
			.SetParametersAndRender(p => p.Add(c => c.Value, "Test Project"));

		cut.FindComponent<MudDatePicker>()
			.SetParametersAndRender(p => p.Add(c => c.Date, DateTime.Today));
		
		cut.FindComponent<MudAutocomplete<Guid?>>()
			.SetParametersAndRender(p => p.Add(c => c.Value, null));
		
		cut.FindComponent<MudForm>().Instance.Validate();

		cut.Render();
		
		cut.FindAll("div")
			.Any(d => d.TextContent.Contains("Je moet een meetnet selecteren."))
			.ShouldBeTrue();
		
		cut.FindAll("div")
			.Any(d => d.TextContent.Contains("Code mag niet leeg zijn."))
			.ShouldBeFalse();
		
		cut.FindAll("div")
			.Any(d => d.TextContent.Contains("Naam mag niet leeg zijn."))
			.ShouldBeFalse();
		
		cut.FindAll("div")
			.Any(d => d.TextContent.Contains("Startdatum mag niet leeg zijn."))
			.ShouldBeFalse();
	}

	[Fact]
	public void ShouldShowSuccessSnackbar_AndAddProject() {
	}

	[Fact]
	public void ShouldShowErrorSnackbar_WithErrorFromService() {
	}
}