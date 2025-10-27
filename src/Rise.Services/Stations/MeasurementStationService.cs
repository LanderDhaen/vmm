using System.Linq;
using Microsoft.EntityFrameworkCore;

using Rise.Domain.Stations;
using Rise.Persistence;
using Rise.Shared.Common;       // Result, QueryRequest
using Rise.Shared.Locations;    // LocationDto
using Rise.Shared.Stations;     // MeasurementStationDto, MeasurementStationResponse

namespace Rise.Services.Stations;

public class MeasurementStationService(ApplicationDbContext dbContext) : IMeasurementStationService
{
    public async Task<Result<MeasurementStationResponse.Index>> GetIndexAsync(QueryRequest.SkipTake request,
        CancellationToken ctx = default)
    {
        IQueryable<MeasurementStation> query = dbContext.MeasurementStations.AsQueryable();

        int totalCount = await query.CountAsync(ctx);

        if (!string.IsNullOrWhiteSpace(request.OrderBy))
        {
            query = query.OrderBy(s => EF.Property<object>(s, request.OrderBy));
        }
        else
        {
            query = query.OrderBy(s => s.Code);
        }

        IQueryable<MeasurementStation> page = query
            .AsNoTracking()
            .Skip(request.Skip)
            .Take(request.Take);

        // Force Queryable.Select to avoid the Enumerable/Queryable ambiguity
        List<MeasurementStationDto.Index> stations = await Queryable
            .Select(page, s => new MeasurementStationDto.Index
            {
                Id = s.Id,
                Code = s.Code,
                Status = s.Status,
                InstalledAt = s.InstalledAt,
                RemovedAt = s.RemovedAt,
                Location = new LocationDto.Index
                {
                    Id = s.Location.Id,
                    Name = s.Location.Name,
                    Address = new AddressDto.Index
                    {
                        AddressLine1 = s.Location.Address.Addressline1,
                        AddressLine2 = s.Location.Address.Addressline2,
                        City = s.Location.Address.City,
                        PostalCode = s.Location.Address.PostalCode,
                        CountryIso = s.Location.Address.CountryIso,
                    },
                    Coordinate = new CoordinateDto.Index
                    {
                        X = s.Location.Coordinate.X,
                        Y = s.Location.Coordinate.Y,
                    }
                }
            })
            .ToListAsync(ctx);

        return Result.Success(new MeasurementStationResponse.Index
        {
            Stations = stations,
            TotalCount = totalCount
        });
    }
}
