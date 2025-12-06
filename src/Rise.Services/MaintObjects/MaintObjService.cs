using Microsoft.EntityFrameworkCore;

using Rise.Domain.Common;
using Rise.Domain.Locations;
using Rise.Domain.MaintObjects;
using Rise.Persistence;
using Rise.Services.Identity;
using Rise.Shared.Common;
using Rise.Shared.Locations;
using Rise.Shared.MaintObjects;

using SharedType = Rise.Shared.MaintObjects.MaintObjType;
using SharedStatus = Rise.Shared.MaintObjects.MaintObjStatus;

namespace Rise.Services.MaintObjects;

/// <summary>
///     Service for maintenance objects. Note the use of <see cref="ISessionContextProvider" /> to get the current user in
///     this layer
///     of the application.
/// </summary>
/// <param name="dbContext"></param>
/// <param name="sessionContextProvider"></param>
public class MaintObjService(ApplicationDbContext dbContext, ISessionContextProvider sessionContextProvider)
	: IMaintObjService {
	public async Task<Result<MaintObjResponse.Index>> GetIndexAsync(QueryRequest.SkipTake req,
		CancellationToken ctx = default) {
		IQueryable<MaintObj> query = dbContext.MaintObjects.AsQueryable().AsNoTracking();

		int totalCount = await query.CountAsync(ctx);
		
		// Sorting

		if (!string.IsNullOrWhiteSpace(req.OrderBy)) {
			bool desc = req.OrderDescending;
			query = req.OrderBy switch {
				nameof(MaintObj.MaintObjPlan)
					=> desc
						? query.OrderByDescending(d => d.MaintObjPlan.Name)
						: query.OrderBy(d => d.MaintObjPlan.Name),


				nameof(MaintObj.MaintObjStatus)
					=> desc
						? query.OrderByDescending(d => d.MaintObjStatus)
						: query.OrderBy(d => d.MaintObjStatus),

				nameof(MaintObj.Location)
					=> desc
						? query.OrderByDescending(d => d.Location.Name)
						: query.OrderBy(d => d.Location.Name),

				_ => desc
					? query.OrderByDescending(e => EF.Property<object>(e, req.OrderBy))
					: query.OrderBy(e => EF.Property<object>(e, req.OrderBy))
			};
		} else {
			query = query.OrderBy(d => d.Id);
		}
		
		// Paginating

		query = query
			.Skip(req.Skip)
			.Take(req.Take);
		
		// Selecting

		List<MaintObjDto.Index> maintObjects = await query
			.Select(m => new MaintObjDto.Index {
				Id = m.Id,
				Name = m.Name,
				SerialNumber = m.SerialNumber,
				Brand = m.Brand,
				MaintObjType = (SharedType)m.MaintObjType,
				MaintObjStatus = (SharedStatus)m.MaintObjStatus,
				Location = m.Location.Name,
			}).ToListAsync(ctx);

		return Result.Success(new MaintObjResponse.Index {
			MaintObjects = maintObjects,
			TotalCount = totalCount
		});
	}

	public async Task<Result<MaintObjResponse.Create>> CreateAsync(MaintObjRequest.Create request,
		CancellationToken ctx = default) {
		if (await dbContext.MaintObjects.AnyAsync(x => x.SerialNumber == request.SerialNumber)) {
			Log.Warning("Onderhoudsobject met serienummer '{SerialNumber}' bestaat al.", request.SerialNumber);
			return Result.Conflict($"Onderhoudsobject met serienummer '{request.SerialNumber}' bestaat al.");
		}

		Location? location = await dbContext.Locations
			.FirstOrDefaultAsync(x => x.Id == request.LocationId);

		if (location is null) {
			return Result.NotFound("De locatie werd niet gevonden.");
		}

		Domain.MaintObjects.MaintObjPlan? type = await dbContext.MaintObjPlans
			.FirstOrDefaultAsync(s => s.Name == request.MaintObjType.ToString(), ctx);

		if (type is null) {
			return Result.NotFound("Het onderhoudstype werd niet gevonden.");
		}
		
		// TODO: Update the form (edit and create)
		
		MaintObj mo = new(request.Name, request.SerialNumber, Domain.MaintObjects.MaintObjType.Battery , type, request.Brand, location);

		dbContext.MaintObjects.Add(mo);

		await dbContext.SaveChangesAsync(ctx);

		return Result.Created(new MaintObjResponse.Create {
			MaintObjId = mo.Id
		});
	}

	public async Task<Result<MaintObjResponse.Detail>> GetDetailAsync(Guid maintObjId, CancellationToken ct = default) {
		
		IQueryable<MaintObj> query = dbContext.MaintObjects.AsQueryable().AsNoTracking();
		
		MaintObjDto.Detail? maintObj = await query
			.Where(m => m.Id == maintObjId)
			.Select(m => new MaintObjDto.Detail {
				Id =  m.Id,
				Name = m.Name,
				SerialNumber = m.SerialNumber,
				Brand = m.Brand,
				MaintObjType = (SharedType)m.MaintObjType,
				MaintObjStatus = (SharedStatus)m.MaintObjStatus,
				Location = new LocationDto.Detail {
					Id = m.Location.Id,
					Name = m.Location.Name,
					AddressLine1 = m.Location.Address.Addressline1,
					AddressLine2 = m.Location.Address.Addressline2,
					City = m.Location.Address.City,
					PostalCode = m.Location.Address.PostalCode,
					Country = m.Location.Address.CountryIso,
					X = m.Location.Coordinate.X,
					Y = m.Location.Coordinate.Y,
				},
				InstalledAt = m.Installation != null ? m.Installation.InstalledAt : null,
				RemovedAt = m.Installation != null ? m.Installation.RemovedAt : null,
				MaintenanceIntervalDays = m.MaintObjPlan.MaintenanceIntervalDays,
				CalibrationIntervalDays = m.MaintObjPlan.CalibrationIntervalDays,
				
			}).SingleOrDefaultAsync(ct);

		if (maintObj is null) {
			return Result.NotFound($"Onderhoudsobject met Id '{maintObjId}' was niet gevonden.");
		}

		return Result.Success(new MaintObjResponse.Detail {
			MaintObj = maintObj
		});
	}

	public async Task<Result> EditAsync(MaintObjRequest.Edit req, CancellationToken ctx = default) {
		MaintObj? maintObj = await dbContext.MaintObjects
			.SingleOrDefaultAsync(x => x.Id == req.MaintObjId, ctx);

		if (maintObj is null) {
			return Result.NotFound($"Onderhoudsobject met Id '{req.MaintObjId}' was niet gevonden.");
		}

		if (await dbContext.MaintObjects.AnyAsync(x => x.SerialNumber == req.SerialNumber && x.Id != req.MaintObjId)) {
			Log.Warning("Onderhoudsobject met serienummer '{SerialNumber}' bestaat al.", req.SerialNumber);
			return Result.Conflict($"Onderhoudsobject met serienummer '{req.SerialNumber}' bestaat al.");
		}

		Location? location = await dbContext.Locations
			.FirstOrDefaultAsync(x => x.Id == req.LocationId);

		if (location is null) {
			return Result.NotFound("De locatie werd niet gevonden.");
		}

		Domain.MaintObjects.MaintObjPlan? type = await dbContext.MaintObjPlans
			.FirstOrDefaultAsync(s => s.Name == req.MaintObjType.ToString(), ctx);

		if (type is null) {
			return Result.NotFound("Het onderhoudstype werd niet gevonden.");
		}

		maintObj.Edit(req.Name, req.SerialNumber, type, req.Brand,
			new Installation((DateTime)req.InstalledAt, req.RemovedAt), location);

		await dbContext.SaveChangesAsync(ctx);

		return Result.Success();
	}

	public async Task<Result<MaintObjResponse.BySerial>> GetBySerialAsync(string serialNumber,
		CancellationToken ct = default) {
		if (string.IsNullOrWhiteSpace(serialNumber)) {
			return Result.Invalid(new[]
			{
				new ValidationError
				{
					Identifier = nameof(serialNumber),
					ErrorMessage = "Serienummer is verplicht."
				}
			});
		}

		MaintObj? maintObj = await dbContext.MaintObjects
			.AsNoTracking()
			.FirstOrDefaultAsync(m => m.SerialNumber == serialNumber, ct);

		if (maintObj is null)
			return Result.NotFound();

		return Result.Success(new MaintObjResponse.BySerial {
			Id = maintObj.Id,
			SerialNumber = maintObj.SerialNumber,
			Name = maintObj.Name
		});
	}

}