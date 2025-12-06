using Microsoft.EntityFrameworkCore;

using Rise.Domain.WorkOrders;
using Rise.Persistence;
using Rise.Shared.Common;
using Rise.Shared.MaintObjects;
using Rise.Shared.WorkOrders;

using SharedMainObjType = Rise.Shared.MaintObjects.MaintObjType;
using SharedMaintObjStatus = Rise.Shared.MaintObjects.MaintObjStatus;
using SharedPriority = Rise.Shared.WorkOrders.Priority;
using SharedStatus = Rise.Shared.WorkOrders.WorkOrderStatus;

namespace Rise.Services.WorkOrders;

/// <summary>
///     Service for projects.
/// </summary>
/// <param name="dbContext"></param>
public class WorkOrderService(ApplicationDbContext dbContext) : IWorkOrderService {

	public async Task<Result<WorkOrderResponse.Index>> GetIndexAsync(QueryRequest.SkipTake req,
		CancellationToken ctx = default) {

		IQueryable<WorkOrder> query = dbContext.WorkOrders.AsQueryable().AsNoTracking();

		int totalCount = await query.CountAsync(ctx);

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
				Status = (SharedStatus)p.Status,
				Priority = (SharedPriority)p.Priority,
				StartDate = p.DateRange != null ? p.DateRange.StartDate : null,
				EndDate = p.DateRange != null ? p.DateRange.EndDate : null,
				Technician = !Equals(p.Technician, null) ? p.Technician.FirstName + " " + p.Technician.LastName : null
			}).ToListAsync(ctx);

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

				MaintObj = new MaintObjDto.Index {
					Id = w.MaintObj.Id,
					Name = w.MaintObj.Name,
					SerialNumber = w.MaintObj.SerialNumber,
					Brand = w.MaintObj.Brand,
					MaintObjType = (SharedMainObjType)w.MaintObj.MaintObjType,
					MaintObjStatus = (SharedMaintObjStatus)w.MaintObj.MaintObjStatus,
					Location = w.MaintObj.Location.Name
				}
			})
			.FirstOrDefaultAsync(ctx);

		if (workOrder is null) {
			return Result.NotFound($"Werkbon met Id '{workOrderId}' was niet gevonden.");
		}

		return Result.Success(new WorkOrderResponse.Detail { WorkOrder = workOrder });
	}

}