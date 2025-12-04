using Microsoft.EntityFrameworkCore;

using Rise.Domain.Networks;
using Rise.Persistence;
using Rise.Services.Identity;
using Rise.Shared.Common;
using Rise.Shared.Zones;

namespace Rise.Services.Zones;

/// <summary>
///     Service for zones.
/// </summary>
public class ZoneService(ApplicationDbContext dbContext, ISessionContextProvider sessionContextProvider)
	: IZoneService {
	public async Task<Result<ZoneResponse.Index>> GetIndexAsync(QueryRequest.SkipTake request,
		CancellationToken ctx = default) {
		IQueryable<Zone> query = dbContext.Zones.AsQueryable();

		if (!string.IsNullOrWhiteSpace(request.OrderBy)) {
			query = query.OrderBy(p => EF.Property<object>(p, request.OrderBy));
		} else {
			query = query.OrderBy(p => p.Name);
		}

		IQueryable<Zone> page = query
			.AsNoTracking()
			.Skip(request.Skip)
			.Take(request.Take);

		List<ZoneDto.Index> zones = await page
			.Select(p => new ZoneDto.Index {
				Id = p.Id,
				Name = p.Name
			})
			.ToListAsync(ctx);

		return Result.Success(new ZoneResponse.Index {
			zones = zones
		});
	}
}