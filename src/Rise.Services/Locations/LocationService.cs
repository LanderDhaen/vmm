using Microsoft.EntityFrameworkCore;

using Rise.Domain.Locations;
using Rise.Persistence;
using Rise.Services.Identity;
using Rise.Shared.Common;
using Rise.Shared.Locations;

namespace Rise.Services.Locations;

/// <summary>
///     Service for locations. Note the use of <see cref="ISessionContextProvider" /> to get the current user in this layer
///     of the application.
/// </summary>
/// <param name="dbContext"></param>
/// <param name="sessionContextProvider"></param>
public class LocationService(ApplicationDbContext dbContext) : ILocationService {
	public async Task<Result<LocationResponse.Index>> GetIndexAsync(QueryRequest.SkipTake request,
		CancellationToken ctx = default) {
		IQueryable<Location> query = dbContext.Locations.AsQueryable();

		if (request.Filters.TryGetValue("isStockLocation", out object? stockObj) && stockObj is bool stock) {
			query = query.Where(l => l.IsStockLocation == stock);
		}

		if (!string.IsNullOrWhiteSpace(request.OrderBy)) {
			query = query.OrderBy(l => EF.Property<object>(l, request.OrderBy));
		} else {
			query = query.OrderBy(l => l.Name);
		}

		List<LocationDto.Index> locations = await query.AsNoTracking()
			.Skip(request.Skip)
			.Take(request.Take)
			.Select(l => new LocationDto.Index {
				Id = l.Id,
				Name = l.Name,
				Address = new AddressDto.Index {
					AddressLine1 = l.Address.Addressline1,
					AddressLine2 = l.Address.Addressline2,
					City = l.Address.City,
					PostalCode = l.Address.PostalCode,
					CountryIso = l.Address.CountryIso
				},
				Coordinate = new CoordinateDto.Index {
					X = l.Coordinate.X,
					Y = l.Coordinate.Y
				}
			})
			.ToListAsync(ctx);

		return Result.Success(new LocationResponse.Index {
			Locations = locations
		});
	}
}