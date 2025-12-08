using FastEndpoints.Testing;

using Rise.Services.Tests.Common;

namespace Rise.Services.Tests.Projects;

public class ProjectEndpointTests(ApiFixture app) : TestBase<ApiFixture> {
	[Fact]
	public async Task Get_Article_List() {
		var (rsp, res) = await app.Client.GETAsync<>();

		rsp.IsSuccessStatusCode.Should().BeTrue();
	}
}