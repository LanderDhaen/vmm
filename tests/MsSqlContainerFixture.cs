using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Xunit;

public class MsSqlContainerFixture : IAsyncLifetime
{
	public MsSqlContainer Container { get; }

	public MsSqlContainerFixture()
	{
		Container = new MsSqlBuilder()
			.WithPassword("yourStrong(!)Password")     // MSSQL requires strong password
			.Build();
	}

	public async Task InitializeAsync()
	{
		await Container.StartAsync();
	}

	public async Task DisposeAsync()
	{
		await Container.DisposeAsync();
	}
}