using Microsoft.EntityFrameworkCore;

using Rise.Domain.Devices;
using Rise.Persistence;
using Rise.Services.Identity;
using Rise.Shared.Common;
using Rise.Shared.Devices;

using SharedStatus = Rise.Shared.Projects.Status;
using SharedType = Rise.Shared.Devices.DeviceType;


namespace Rise.Services.Devices;

/// <summary>
///     Service for devices. Note the use of <see cref="ISessionContextProvider" /> to get the current user in this layer
///     of the application.
/// </summary>
/// <param name="dbContext"></param>
/// <param name="sessionContextProvider"></param>
public class DeviceService(ApplicationDbContext dbContext, ISessionContextProvider sessionContextProvider)
	: IDeviceService {
	public async Task<Result<DeviceResponse.Index>> GetIndexAsync(QueryRequest.SkipTake req,
		CancellationToken ctx = default) {
		IQueryable<Device> query = dbContext.Devices.AsQueryable();

		int totalCount = await query.CountAsync(ctx);

		if (!string.IsNullOrWhiteSpace(req.OrderBy)) {
			bool desc = req.OrderDescending;
			switch (req.OrderBy) {
			// TODO
			default:
				query = desc
					? query.OrderByDescending(e => EF.Property<object>(e, req.OrderBy))
					: query.OrderBy(e => EF.Property<object>(e, req.OrderBy));
				break;
			}
		} else {
			query = query.OrderBy(d => d.Id);
		}

		IQueryable<Device> page = query
			.AsNoTracking()
			.Skip(req.Skip)
			.Take(req.Take);
		List<DeviceDto.Index> devices = await page
			.Select(d => new DeviceDto.Index {
				Id = d.Id,
				Name = d.Name,
				SerialNumber = d.SerialNumber,
				DeviceType = (SharedType)d.DeviceType,
				Status = (SharedStatus)d.Status
			})
			.ToListAsync(ctx);

		return Result.Success(new DeviceResponse.Index {
			Devices = devices,
			TotalCount = totalCount
		});
	}
}