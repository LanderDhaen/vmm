using Microsoft.EntityFrameworkCore;

using Rise.Domain.Networks;
using Rise.Domain.Projects;
using Rise.Persistence;
using Rise.Services.Identity;
using Rise.Shared.Common;
using Rise.Shared.Networks;
using Rise.Shared.Projects;

using SharedStatus = Rise.Shared.Projects.Status;
using Status = Rise.Domain.Projects.Status;

namespace Rise.Services.Projects;

/// <summary>
///     Service for projects. Note the use of <see cref="ISessionContextProvider" /> to get the current user in this layer
///     of the application.
/// </summary>
/// <param name="dbContext"></param>
/// <param name="sessionContextProvider"></param>
public class ProjectService(ApplicationDbContext dbContext, ISessionContextProvider sessionContextProvider)
	: IProjectService {
	public async Task<Result<ProjectResponse.Create>> CreateAsync(ProjectRequest.Create request,
		CancellationToken ctx = default) {
		if (await dbContext.Projects.AnyAsync(x => x.Code == request.Code)) {
			Log.Warning("Project met code '{Code}' bestaat al.", request.Code);
			return Result.Conflict($"Project met code '{request.Code}' bestaat al.");
		}

		DateOnly startdate = DateOnly.FromDateTime(request.StartDate!.Value);
		MeasurementNetwork? measurementNetwork = await dbContext.MeasurementNetworks
			.FirstOrDefaultAsync(x => x.Id == request.MeasurementNetworkId);

		if (measurementNetwork == null) {
			return Result.NotFound("Meetnet niet gevonden.");
		}

		Project p = new(request.Name, request.Code, new DateRange(startdate), measurementNetwork);

		dbContext.Projects.Add(p);

		await dbContext.SaveChangesAsync(ctx);

		return Result.Created(new ProjectResponse.Create {
			ProjectId = p.Id
		});
	}

	public async Task<Result> EditAsync(ProjectRequest.Edit req, CancellationToken ctx = default) {
		Project? project = await dbContext.Projects
			.SingleOrDefaultAsync(x => x.Id == req.ProjectId, ctx);

		if (project is null) {
			return Result.NotFound($"Project met Id '{req.ProjectId}' was niet gevonden.");
		}

		if (await dbContext.Projects.AnyAsync(x => x.Code == req.Code && x.Id != req.ProjectId)) {
			Log.Warning("Project met code '{Code}' bestaat al.", req.Code);
			return Result.Conflict($"Project met code '{req.Code}' bestaat al.");
		}

		DateOnly startdate = DateOnly.FromDateTime(req.StartDate!.Value);
		DateOnly? enddate = req.EndDate.HasValue ? DateOnly.FromDateTime(req.EndDate.Value) : null;
		MeasurementNetwork? measurementNetwork = await dbContext.MeasurementNetworks
			.FirstOrDefaultAsync(x => x.Id == req.MeasurementNetworkId);

		if (measurementNetwork == null) {
			return Result.NotFound("Meetnet niet gevonden.");
		}

		project.Edit(req.Name, req.Code, new DateRange(startdate, enddate), measurementNetwork);

		await dbContext.SaveChangesAsync(ctx);

		return Result.Success();
	}

	public async Task<Result<ProjectResponse.Index>> GetIndexAsync(QueryRequest.SkipTake req,
		CancellationToken ctx = default) {
		IQueryable<Project> query = dbContext.Projects.AsQueryable();

		int totalCount = await query.CountAsync(ctx);
		
		if (!string.IsNullOrWhiteSpace(req.OrderBy)) {
			bool desc = req.OrderDescending;
			switch (req.OrderBy) {
			
			case nameof(DateRange.StartDate):
				query = desc ? query.OrderByDescending(p => p.DateRange.StartDate) : query.OrderBy(p => p.DateRange.StartDate);
				break;
			case nameof(DateRange.EndDate):
				query = desc ? query.OrderByDescending(p => p.DateRange.EndDate) : query.OrderBy(p => p.DateRange.EndDate);
				break;
			case nameof(MeasurementNetwork):
				query = desc ? query.OrderByDescending(p => p.MeasurementNetwork.Name) : query.OrderBy(p => p.MeasurementNetwork.Name);
				break;
			case nameof(Zone):
				query = desc ? query.OrderByDescending(p => p.MeasurementNetwork.Zone.Name) : query.OrderBy(p => p.MeasurementNetwork.Zone.Name);
				break;
			default:
					query = desc
						? query.OrderByDescending(e => EF.Property<object>(e, req.OrderBy))
						: query.OrderBy(e => EF.Property<object>(e, req.OrderBy));
					break;
			}
		} else {
			query = query.OrderBy(p => p.Id);
		}

		IQueryable<Project> page = query
			.AsNoTracking()
			.Skip(req.Skip)
			.Take(req.Take);

		List<ProjectDto.Index> projects = await page
			.Select(p => new ProjectDto.Index {
				Id = p.Id,
				Name = p.Name,
				Code = p.Code,
				StartDate = p.DateRange.StartDate,
				EndDate = p.DateRange.EndDate,
				MeasurementNetwork = p.MeasurementNetwork.Name,
				Zone = p.MeasurementNetwork.Zone.Name,
				Status = (SharedStatus)p.Status
			})
			.ToListAsync(ctx);

		return Result.Success(new ProjectResponse.Index {
			Projects = projects,
			TotalCount = totalCount
		});
	}

	public async Task<Result<ProjectResponse.Detail>> GetDetailAsync(Guid projectId, CancellationToken ctx = default) {
		Project? p = await dbContext.Projects
			.AsNoTracking()
			.Include(p => p.MeasurementNetwork)
			.ThenInclude(mn => mn.Zone)
			.SingleOrDefaultAsync(p => p.Id == projectId, ctx);

		if (p is null) {
			return Result.NotFound($"Project with Id '{projectId}' was not found.");
		}

		ProjectDto.Detail projectDto = new() {
			Id = p.Id,
			Name = p.Name,
			Code = p.Code,
			StartDate = p.DateRange.StartDate,
			EndDate = p.DateRange.EndDate,
			MeasurementNetwork = new MeasurementNetworkDto.Index {
				Id = p.MeasurementNetwork.Id,
				Name = p.MeasurementNetwork.Name,
				Zone = p.MeasurementNetwork.Zone.Name
			},
			Zone = p.MeasurementNetwork.Zone.Name,
			Status = (SharedStatus)p.Status
		};

		return Result.Success(new ProjectResponse.Detail {
			Project = projectDto
		});
	}

	public async Task<Result<ProjectResponse.History>> GetHistoryAsync(Guid projectId, CancellationToken ctx = default) {

		IQueryable<Project> query = dbContext.Projects.AsQueryable().AsNoTracking();

		ProjectDto.History? project = await query
			.Where(p => p.Id == projectId)
			.Select(p => new ProjectDto.History {
				Id = p.Id,
				Name = p.Name,
				Code = p.Code,
				StartDate = p.DateRange.StartDate,
				EndDate = p.DateRange.EndDate,
				ProjectHistories = p.ProjectHistories
					.OrderByDescending(h => h.ValidityRange.ValidTo)
					.Select(h => new ProjectHistoryDto.Index {
						Id = h.Id,
						StartDate = h.DateRange.StartDate,
						EndDate = h.DateRange.EndDate,
						ValidFrom = h.ValidityRange.ValidFrom,
						ValidTo = h.ValidityRange.ValidTo,
					}).ToList()
			})
			.FirstOrDefaultAsync(ctx);

		if (project is null) {
			return Result.NotFound($"Project with Id '{projectId}' was not found");
		}

		return Result.Success(new ProjectResponse.History {
			Project = project
		});
	}


	public async Task<Result> UpdateStatusAsync(ProjectRequest.UpdateStatus req, CancellationToken ctx = default) {
		Project? project = await dbContext.Projects
			.SingleOrDefaultAsync(x => x.Id == req.ProjectId, ctx);

		if (project is null) {
			return Result.NotFound($"Project with Id '{req.ProjectId}' was not found.");
		}

		// Technician? loggedInTechnician = await dbContext.Technicians
		// 	.SingleOrDefaultAsync(x => x.AccountId == sessionContextProvider.User!.GetUserId(), ctx);
		//
		// if (loggedInTechnician is null) {
		// 	return Result.Unauthorized("You are not authorized to edit this project.");
		// }


		project.UpdateStatus((Status)req.NewStatus);

		await dbContext.SaveChangesAsync(ctx);

		return Result.Success();
	}
}