using System.Linq;
using Microsoft.EntityFrameworkCore;

using Rise.Domain.Stations;
using Rise.Persistence;
using Rise.Shared.Common;
using Rise.Shared.Locations;
using Rise.Shared.Stations;

namespace Rise.Services.Stations;

public class MeasurementStationService(ApplicationDbContext dbContext) : IMeasurementStationService
{
    public async Task<Result<MeasurementStationResponse.Index>> GetIndexAsync(
        QueryRequest.SkipTake request,
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

        List<MeasurementStationDto.Index> stations = await page
            .Select(s => new MeasurementStationDto.Index
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
                        City        = s.Location.Address.City,
                        PostalCode  = s.Location.Address.PostalCode,
                        CountryIso  = s.Location.Address.CountryIso,
                    },
                    Coordinate = new CoordinateDto.Index
                    {
                        X = s.Location.Coordinate.X,
                        Y = s.Location.Coordinate.Y,
                    }
                },
                
                ProjectId   = s.ProjectId,
                ProjectName = s.Project.Name,
                ProjectCode = s.Project.Code,
            })
            .ToListAsync(ctx);

        return Result.Success(new MeasurementStationResponse.Index
        {
            Stations = stations,
            TotalCount = totalCount
        });
    }
	
    public async Task<Result<Guid>> CreateAsync(CreateMeasurementStationRequest request, CancellationToken ct = default)
    {
        var entity = new MeasurementStation
        {
            Code = request.Code,
            Status = request.Status,
            InstalledAt = request.InstalledAt,
            LocationId = request.LocationId,
            ProjectId = request.ProjectId, // NEW
        };

        dbContext.MeasurementStations.Add(entity);
        await dbContext.SaveChangesAsync(ct);

        return Result.Success(entity.Id);
    }
	
    public async Task<Result> UpdateAsync(Guid id, UpdateMeasurementStationRequest request, CancellationToken ct = default)
    {
        var entity = await dbContext.MeasurementStations
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        if (entity == null)
            return Result.NotFound();

        entity.Code = request.Code;
        entity.Status = request.Status;
        entity.InstalledAt = request.InstalledAt;
        entity.RemovedAt = request.RemovedAt;
        entity.LocationId = request.LocationId;
        entity.ProjectId = request.ProjectId; 

        await dbContext.SaveChangesAsync(ct);
        return Result.Success();
    }
}
