using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using Rise.Domain.WorkOrders;
using Rise.Persistence;
using Rise.Shared.Common;
using Rise.Shared.Locations;
using Rise.Shared.MaintObjects;
using Rise.Shared.WorkOrders;

using SharedMainObjType = Rise.Shared.MaintObjects.MaintObjType;
using SharedMaintObjStatus = Rise.Shared.MaintObjects.MaintObjStatus;
using SharedPriority = Rise.Shared.WorkOrders.Priority;
using SharedStatus = Rise.Shared.WorkOrders.WorkOrderStatus;
using Status = Rise.Domain.WorkOrders.WorkOrderStatus;

namespace Rise.Services.WorkOrders;

/// <summary>
/// Service for workorders.
/// </summary>
/// <param name="dbContext"></param>
public class WorkOrderService(ApplicationDbContext dbContext) : IWorkOrderService {

	private static FilterSpec? ParseFilterSpec(object? rawValue) {
		if (rawValue is null)
			return null;

		try {
			if (rawValue is not JsonElement je)
				return new FilterSpec(null, rawValue?.ToString());

			if (je.ValueKind != JsonValueKind.Object)
				return new FilterSpec(null, je.ValueKind == JsonValueKind.String ? je.GetString() : je.GetRawText());

			string? op = null;
			string? val = null;

			if (je.TryGetProperty("Operator", out var opProp))
				op = opProp.ValueKind == JsonValueKind.String ? opProp.GetString() : opProp.GetRawText();

			if (je.TryGetProperty("Value", out var vProp))
				val = vProp.ValueKind == JsonValueKind.String ? vProp.GetString() : vProp.GetRawText();

			return new FilterSpec(op, val);
		} catch {
			return null;
		}
	}

	private static bool TryParseDateOnlySimple(string? raw, out DateOnly d) {
		d = default;
		if (string.IsNullOrWhiteSpace(raw)) return false;
		raw = raw.Trim().Trim('"');
		if (DateOnly.TryParseExact(raw, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out d))
			return true;
		return false;
	}

	public async Task<Result<WorkOrderResponse.Index>> GetIndexAsync(QueryRequest.SkipTake req,
		CancellationToken ctx = default) {

		IQueryable<WorkOrder> query = dbContext.WorkOrders
			.Include(w => w.MaintObj)
				.ThenInclude(m => m.Location)
			.Include(w => w.MaintObj)
				.ThenInclude(m => m.MaintObjPlan)
			.Include(w => w.Technician)
			.AsQueryable()
			.AsNoTracking();

		if (!string.IsNullOrWhiteSpace(req.SearchTerm)) {
			var t = req.SearchTerm.Trim().ToLowerInvariant();
			query = query.Where(p =>
				(!string.IsNullOrEmpty(p.Name) && p.Name.ToLower().Contains(t)) ||
				(p.MaintObj != null && p.MaintObj.Name.ToLower().Contains(t)) ||
				(p.Technician != null &&
					((p.Technician.FirstName + " " + p.Technician.LastName).ToLower().Contains(t) ||
					 (p.Technician.LastName + " " + p.Technician.FirstName).ToLower().Contains(t)))
			);
		}

		// Filters
		if (req.Filters != null && req.Filters.Count != 0) {
			var temp = req.Filters
				.Select(kv => (kv.Key, Spec: ParseFilterSpec(kv.Value)))
				.Where(x => x.Spec != null)
				.ToList();

			var parsed = temp.Select(x => (Key: x.Key!, Spec: x.Spec!)).ToList();

			foreach ((string keyOrig, FilterSpec spec) in parsed) {
				var key = keyOrig?.Trim();
				var raw = spec.Value;
				var opCandidate = spec.Operator;
				var opNorm = string.IsNullOrWhiteSpace(opCandidate) ? "Contains" : opCandidate;

				switch (key) {
				case "MaintObjType": {
						if (!string.IsNullOrWhiteSpace(raw)) {
							if (Enum.TryParse<SharedMainObjType>(raw, true, out var mt)) {
								query = query.Where(p => p.MaintObj != null && p.MaintObj.MaintObjType == (Domain.MaintObjects.MaintObjType)mt);
							} else if (int.TryParse(raw, out var n)) {
								query = query.Where(p => p.MaintObj != null && (int)p.MaintObj.MaintObjType == n);
							}
						}
						break;
					}
				case "Status": {
						if (!string.IsNullOrWhiteSpace(raw)) {
							if (Enum.TryParse<Status>(raw, true, out var sRes))
								query = query.Where(p => p.Status == sRes);
							else if (int.TryParse(raw, out var n))
								query = query.Where(p => (int)p.Status == n);
						}
						break;
					}
				case "Technician": {
						var t = (raw ?? string.Empty).Trim().ToLowerInvariant();
						if (!string.IsNullOrEmpty(t)) {
							query = query.Where(p => p.Technician != null && EF.Functions.Like((p.Technician.FirstName + " " + p.Technician.LastName).ToLower(), $"%{t}%"));
						}
						break;
					}
				case "StartDate": {
						if (opNorm == "IsEmpty")
							query = query.Where(p => p.DateRange == null || p.DateRange.StartDate == default);
						else {
							if (TryParseDateOnlySimple(raw, out var d)) {
								query = opNorm switch {
									"GreaterThanOrEqual" => query.Where(p => p.DateRange != null && p.DateRange.StartDate >= d),
									"LessThanOrEqual" => query.Where(p => p.DateRange != null && p.DateRange.StartDate <= d),
									_ => query.Where(p => p.DateRange != null && p.DateRange.StartDate == d)
								};
							}
						}
						break;
					}
				case "EndDate": {
						if (opNorm == "IsEmpty")
							query = query.Where(p => p.DateRange == null || p.DateRange.EndDate == null);
						else {
							if (TryParseDateOnlySimple(raw, out var d)) {
								query = opNorm switch {
									"GreaterThanOrEqual" => query.Where(p => p.DateRange != null && p.DateRange.EndDate != null && p.DateRange.EndDate >= d),
									"LessThanOrEqual" => query.Where(p => p.DateRange != null && p.DateRange.EndDate != null && p.DateRange.EndDate <= d),
									_ => query.Where(p => p.DateRange != null && p.DateRange.EndDate != null && p.DateRange.EndDate == d)
								};
							}
						}
						break;
					}
				}
			}
		}

		// total count before paging
		int totalCount = await query.CountAsync(ctx);

		// Sorting
		if (!string.IsNullOrWhiteSpace(req.OrderBy)) {
			bool desc = req.OrderDescending;
			var orderBy = req.OrderBy.Trim();
			var key = orderBy.ToLowerInvariant();

			switch (key) {
			case "priority":
				query = desc ? query.OrderByDescending(p => p.Priority) : query.OrderBy(p => p.Priority);
				break;

			case "maintobjtype":
				query = desc
					? query.OrderByDescending(p => p.MaintObj.MaintObjType)
					: query.OrderBy(p => p.MaintObj.MaintObjType);
				break;

			case "maintobj":
				query = desc ? query.OrderByDescending(p => p.MaintObj.Name) : query.OrderBy(p => p.MaintObj.Name);
				break;

			case "name":
				query = desc ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name);
				break;

			case "technician":
				query = desc
					? query.OrderByDescending(p => p.Technician!.LastName).ThenByDescending(p => p.Technician!.FirstName)
					: query.OrderBy(p => p.Technician!.LastName).ThenBy(p => p.Technician!.FirstName);
				break;

			case "status":
				query = desc ? query.OrderByDescending(p => p.Status) : query.OrderBy(p => p.Status);
				break;

			case "startdate":
				query = desc ? query.OrderByDescending(p => p.DateRange != null ? p.DateRange.StartDate : default) : query.OrderBy(p => p.DateRange != null ? p.DateRange.StartDate : default);
				break;

			case "enddate":
				query = desc ? query.OrderByDescending(p => p.DateRange != null ? p.DateRange.EndDate : null) : query.OrderBy(p => p.DateRange != null ? p.DateRange.EndDate : null);
				break;

			default:
				try {
					query = desc ? query.OrderByDescending(e => EF.Property<object>(e, req.OrderBy)) : query.OrderBy(e => EF.Property<object>(e, req.OrderBy));
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
					: null,
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
					Y = w.MaintObj.Location.Coordinate.Y,
				}
			})
			.FirstOrDefaultAsync(ctx);

		if (workOrder is null) {
			return Result.NotFound($"Werkbon met Id '{workOrderId}' was niet gevonden.");
		}

		return Result.Success(new WorkOrderResponse.Detail { WorkOrder = workOrder });
	}

	public async Task<Result> UpdateStatusAsync(WorkOrderRequest.UpdateStatus req, CancellationToken ctx = default) {
		WorkOrder? workOrder = await dbContext.WorkOrders
			.SingleOrDefaultAsync(x => x.Id == req.WorkOrderId, ctx);

		if (workOrder is null) {
			return Result.NotFound($"Werkbon met Id '{req.WorkOrderId}' is niet gevonden.");
		}

		workOrder.UpdateStatus((Status)req.NewStatus);

		await dbContext.SaveChangesAsync(ctx);

		return Result.Success();
	}
}