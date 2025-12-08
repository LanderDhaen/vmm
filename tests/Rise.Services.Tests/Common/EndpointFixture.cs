using FastEndpoints.Testing;

using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace Rise.Services.Tests.Common;

public class EndpointFixture : AppFixture<Program> {
	protected override ValueTask SetupAsync() {
		// place one-time setup for the fixture here
		return ValueTask.CompletedTask;
	}

	protected override void ConfigureApp(IWebHostBuilder a) {
		// do host builder configuration here
	}

	protected override void ConfigureServices(IServiceCollection s) {
		// do test service registration here
	}

	protected override ValueTask TearDownAsync() {
		// do cleanups here
		return ValueTask.CompletedTask;
	}
}