using Ardalis.Result;

using Rise.Shared.Common;
using Rise.Shared.Networks;

namespace Rise.Client.Networks;

public class FakeNetworkService : INetworkService {
	public Task<Result<NetworkResponse.Index>> GetIndexAsync(QueryRequest.SkipTake request,
		CancellationToken ctx = default) {
		IEnumerable<MeasurementNetworkDto.Index> networks = Enumerable.Range(1, 5)
			.Select(i => new MeasurementNetworkDto.Index
				{ Id = Guid.NewGuid(), Name = $"Product {i}", Zone = "Test zone" });

		NetworkResponse.Index wrapper = new() {
			measurementsNetworks = networks,
		};

		return Task.FromResult(Result.Success(wrapper));
	}
}