using Microsoft.EntityFrameworkCore;

using Rise.Domain.Networks;
using Rise.Persistence;
using Rise.Shared.Common;
using Rise.Shared.Networks;

namespace Rise.Services.Networks;

/// <summary>
///     Service for measurement networks.
///     of the application.
/// </summary>
/// <param name="dbContext"></param>
public class NetworkService(ApplicationDbContext dbContext)
	: INetworkService {
	public async Task<Result<NetworkResponse.Index>> GetIndexAsync(QueryRequest.SkipTake request,
		CancellationToken ctx = default) {
		IQueryable<MeasurementNetwork> query = dbContext.MeasurementNetworks.AsQueryable();

		query = !string.IsNullOrWhiteSpace(request.OrderBy)
			? query.OrderBy(p => EF.Property<object>(p, request.OrderBy))
			: query.OrderBy(p => p.Name);

		IQueryable<MeasurementNetwork> page = query
			.AsNoTracking()
			.Skip(request.Skip)
			.Take(request.Take);

		List<MeasurementNetworkDto.Index> networks = await page
			.Select(p => new MeasurementNetworkDto.Index {
				Id = p.Id,
				Name = p.Name,
				Zone = p.Zone.Name
			})
			.ToListAsync(ctx);

		return Result.Success(new NetworkResponse.Index {
			measurementsNetworks = networks
		});
	}
}