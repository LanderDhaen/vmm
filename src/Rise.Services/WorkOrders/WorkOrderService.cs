using Microsoft.EntityFrameworkCore;

using Rise.Domain.MaintObjects;
using Rise.Domain.WorkOrders;
using Rise.Persistence;
using Rise.Shared.Common;
using Rise.Shared.Locations;
using Rise.Shared.MaintObjects;
using Rise.Shared.Stations;
using Rise.Shared.WorkOrders;

using SharedMainObjType = Rise.Shared.MaintObjects.MaintObjType;
using SharedMaintObjStatus = Rise.Shared.MaintObjects.MaintObjStatus;
using SharedPriority = Rise.Shared.WorkOrders.Priority;
using SharedStatus = Rise.Shared.WorkOrders.WorkOrderStatus;
using Status = Rise.Domain.WorkOrders.WorkOrderStatus;

namespace Rise.Services.WorkOrders;

/// <summary>
///     Service for workorders.
/// </summary>
/// <param name="dbContext"></param>
public class WorkOrderService(ApplicationDbContext dbContext) : IWorkOrderService {
	public async Task<Result<WorkOrderResponse.Index>> GetIndexAsync(QueryRequest.SkipTake req,
		CancellationToken ctx = default) {
		IQueryable<WorkOrder> query = dbContext.WorkOrders.AsQueryable().AsNoTracking();


		int totalCount = await query.CountAsync(ctx);

		// Sorting
		if (!string.IsNullOrWhiteSpace(req.OrderBy)) {
			bool desc = req.OrderDescending;
			string orderBy = req.OrderBy.Trim();
			string key = orderBy.ToLowerInvariant();

			switch (key) {
			case "priority":
				query = desc
					? query.OrderBy(p => p.Priority)
					: query.OrderByDescending(p => (SharedPriority)p.Priority);
				break;

			case "maintobjtype":
				query = desc
					? query.OrderByDescending(p => p.MaintObj.MaintObjType)
					: query.OrderBy(p => (SharedMainObjType)p.MaintObj.MaintObjType);
				break;

			case "maintobj":
				query = desc ? query.OrderByDescending(p => p.MaintObj.Name) : query.OrderBy(p => p.MaintObj.Name);
				break;

			case "name":
				query = desc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name);
				break;

			case "technician":
				query = desc
					? query.OrderByDescending(p => p.Technician!.LastName)
						.ThenByDescending(p => p.Technician!.FirstName)
					: query.OrderBy(p => p.Technician!.LastName)
						.ThenBy(p => p.Technician!.FirstName);
				break;

			case "status":
				query = desc ? query.OrderByDescending(p => p.Status) : query.OrderBy(p => (SharedStatus)p.Status);
				break;

			case "startdate":
				query = desc
					? query.OrderByDescending(p => p.DateRange != null ? p.DateRange.StartDate : default)
					: query.OrderBy(p => p.DateRange != null ? p.DateRange.StartDate : default);
				break;

			case "enddate":
				query = desc
					? query.OrderByDescending(p => p.DateRange != null ? p.DateRange.EndDate : null)
					: query.OrderBy(p => p.DateRange != null ? p.DateRange.EndDate : null);
				break;

			default:
				try {
					query = desc
						? query.OrderByDescending(e => EF.Property<object>(e, req.OrderBy))
						: query.OrderBy(e => EF.Property<object>(e, req.OrderBy));
				} catch {
					query = query.OrderBy(w => w.Id);
				}

				break;
			}
		} else {
			query = query.OrderBy(w => w.Id);
		}

		// Paginating
		query = query
			.Skip(req.Skip)
			.Take(req.Take);

		// Selecting
		List<WorkOrderDto.Index> workOrders = await query
			.Select(p => new WorkOrderDto.Index {
				Id = p.Id,
				Name = p.Name,
				MaintObj = p.MaintObj.Name,
				MaintObjType = (SharedMainObjType)p.MaintObj.MaintObjType,
				Priority = (SharedPriority)p.Priority,
				Status = (SharedStatus)p.Status,
				StartDate = p.DateRange != null ? p.DateRange.StartDate : null,
				EndDate = p.DateRange != null ? p.DateRange.EndDate : null,
				Technician = p.Technician != null
					? p.Technician.FirstName + " " + p.Technician.LastName
					: null
			})
			.ToListAsync(ctx);

		return Result.Success(new WorkOrderResponse.Index {
			WorkOrders = workOrders,
			TotalCount = totalCount
		});
	}

	public async Task<Result<WorkOrderResponse.Detail>> GetDetailAsync(
		Guid workOrderId,
		CancellationToken ctx = default
	) {
		WorkOrderDto.Detail? workOrder = await dbContext.WorkOrders
			.AsNoTracking()
			.Where(w => w.Id == workOrderId)
			.Select(w => new WorkOrderDto.Detail {
				Id = w.Id,
				Name = w.Name,
				Description = w.Description,
				Priority = (SharedPriority)w.Priority,
				Status = (SharedStatus)w.Status,
				StartDate = w.DateRange != null ? w.DateRange.StartDate : null,
				EndDate = w.DateRange != null ? w.DateRange.EndDate : null,
				Technician = w.Technician != null
					? w.Technician.FirstName + " " + w.Technician.LastName
					: null,

				MaintObj = new MaintObjDto.Detail {
					Id = w.MaintObj.Id,
					Name = w.MaintObj.Name,
					SerialNumber = w.MaintObj.SerialNumber,
					Brand = w.MaintObj.Brand,
					MaintObjType = (SharedMainObjType)w.MaintObj.MaintObjType,
					MaintObjStatus = (SharedMaintObjStatus)w.MaintObj.MaintObjStatus,

					Location = new LocationDto.Detail {
						Id = w.MaintObj.Location.Id,
						Name = w.MaintObj.Location.Name,
						AddressLine1 = w.MaintObj.Location.Address.Addressline1,
						AddressLine2 = w.MaintObj.Location.Address.Addressline2,
						City = w.MaintObj.Location.Address.City,
						PostalCode = w.MaintObj.Location.Address.PostalCode,
						Country = w.MaintObj.Location.Address.CountryIso,
						X = w.MaintObj.Location.Coordinate.X,
						Y = w.MaintObj.Location.Coordinate.Y
					},

					StationCode = w.MaintObj.Station != null ? w.MaintObj.Station.Code : null,
					Station = w.MaintObj.Station != null
						? new MeasurementStationDto.Detail {
							Id = w.MaintObj.Station.Id,
							Code = w.MaintObj.Station.Code,
							Location = new LocationDto.Detail {
								Id = w.MaintObj.Station.Location.Id,
								Name = w.MaintObj.Station.Location.Name,
								AddressLine1 = w.MaintObj.Station.Location.Address.Addressline1,
								AddressLine2 = w.MaintObj.Station.Location.Address.Addressline2,
								City = w.MaintObj.Station.Location.Address.City,
								PostalCode = w.MaintObj.Station.Location.Address.PostalCode,
								Country = w.MaintObj.Station.Location.Address.CountryIso,
								X = w.MaintObj.Station.Location.Coordinate.X,
								Y = w.MaintObj.Station.Location.Coordinate.Y
							}
						}
						: null,
					InstalledAt = w.MaintObj.Installation != null ? w.MaintObj.Installation.InstalledAt : null,
					RemovedAt = w.MaintObj.Installation != null ? w.MaintObj.Installation.RemovedAt : null,
					MaintenanceIntervalDays = w.MaintObj.MaintObjPlan.MaintenanceIntervalDays,
					CalibrationIntervalDays = w.MaintObj.MaintObjPlan.CalibrationIntervalDays
				},

				Location = new LocationDto.Detail {
					Id = w.MaintObj.Location.Id,
					Name = w.MaintObj.Location.Name,
					AddressLine1 = w.MaintObj.Location.Address.Addressline1,
					AddressLine2 = w.MaintObj.Location.Address.Addressline2,
					City = w.MaintObj.Location.Address.City,
					PostalCode = w.MaintObj.Location.Address.PostalCode,
					Country = w.MaintObj.Location.Address.CountryIso,
					X = w.MaintObj.Location.Coordinate.X,
					Y = w.MaintObj.Location.Coordinate.Y
				}
			})
			.FirstOrDefaultAsync(ctx);

		if (workOrder is null) {
			return Result.NotFound($"Werkbon met Id '{workOrderId}' was niet gevonden.");
		}

		return Result.Success(new WorkOrderResponse.Detail { WorkOrder = workOrder });
	}

	public async Task<Result> UpdateStatusAsync(
		WorkOrderRequest.UpdateStatus req,
		CancellationToken ctx = default
	) {
		WorkOrder? workOrder = await dbContext.WorkOrders
			.Include(x => x.MaintObj)
			.SingleOrDefaultAsync(x => x.Id == req.WorkOrderId, ctx);

		if (workOrder is null) {
			return Result.NotFound($"Werkbon met Id '{req.WorkOrderId}' is niet gevonden.");
		}

		try {
			workOrder.UpdateStatus((Status)req.NewStatus);
		} catch (ArgumentException ex) {
			return Result.Conflict(ex.Message);
		}

		await dbContext.SaveChangesAsync(ctx);

		return Result.Success();
	}


	public async Task<Result<WorkOrderResponse.Create>> CreateAsync(
		WorkOrderRequest.Create req,
		CancellationToken ctx = default
	) {
		MaintObj? maintObj = await dbContext.MaintObjects
			.SingleOrDefaultAsync(x => x.Id == req.MaintObjId, ctx);

		if (maintObj is null) {
			return Result.NotFound(
				$"Onderhoudsobject met Id '{req.MaintObjId}' is niet gevonden."
			);
		}

		WorkOrder workOrder = new(
			req.Name,
			req.Description,
			(Rise.Domain.WorkOrders.Priority)req.Priority,
			maintObj
		);

		dbContext.WorkOrders.Add(workOrder);
		await dbContext.SaveChangesAsync(ctx);

		return Result.Created(new WorkOrderResponse.Create {
			WorkOrderId = workOrder.Id
		});
	}
}