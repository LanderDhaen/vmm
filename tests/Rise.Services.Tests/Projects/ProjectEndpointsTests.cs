using FastEndpoints.Testing;

using Rise.Services.Tests.Common;

namespace Rise.Services.Tests.Projects;

public class ProjectEndpointsTests(EndpointFixture endpoint) : TestBase<EndpointFixture> {
	[Fact]
	public async Task Get_Article_List() {
		var (rsp, res) = await endpoint.Client.GETAsync<>();

		rsp.IsSuccessStatusCode.Should().BeTrue();
	}
}