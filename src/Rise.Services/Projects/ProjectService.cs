using Microsoft.EntityFrameworkCore;

using Rise.Domain.Networks;
using Rise.Domain.Projects;
using Rise.Persistence;
using Rise.Services.Identity;
using Rise.Shared.Common;
using Rise.Shared.Networks;
using Rise.Shared.Projects;

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

	public async Task<Result<ProjectResponse.Index>> GetIndexAsync(QueryRequest.SkipTake request,
		CancellationToken ctx = default) {
		IQueryable<Project> query = dbContext.Projects.AsQueryable();

		int totalCount = await query.CountAsync(ctx);

		if (!string.IsNullOrWhiteSpace(request.OrderBy)) {
			query = query.OrderBy(p => EF.Property<object>(p, request.OrderBy));
		} else {
			query = query.OrderBy(p => p.Name);
		}

		IQueryable<Project> page = query
			.AsNoTracking()
			.Skip(request.Skip)
			.Take(request.Take);

		List<ProjectDto.Index> projects = await page
			.Select(p => new ProjectDto.Index {
				Id = p.Id,
				Name = p.Name,
				Code = p.Code,
				StartDate = p.DateRange.StartDate,
				EndDate = p.DateRange.EndDate,
				MeasurementNetwork = p.MeasurementNetwork.Name,
				Zone = p.MeasurementNetwork.Zone.Name
			})
			.ToListAsync(ctx);

		return Result.Success(new ProjectResponse.Index {
			Projects = projects,
			TotalCount = totalCount
		});
	}

	public async Task<Result<ProjectResponse.Detail>> GetDetailAsync(Guid projectId, CancellationToken ctx = default) {
		Project? project = await dbContext.Projects
			.AsNoTracking()
			.Include(p => p.MeasurementNetwork)
			.ThenInclude(mn => mn.Zone)
			.SingleOrDefaultAsync(p => p.Id == projectId, ctx);

		if (project is null) {
			return Result.NotFound($"Project with Id '{projectId}' was not found.");
		}

		ProjectDto.Detail projectDto = new() {
			Id = project.Id,
			Name = project.Name,
			Code = project.Code,
			StartDate = project.DateRange.StartDate,
			EndDate = project.DateRange.EndDate,
			MeasurementNetwork = new MeasurementNetworkDto.Index {
				Id = project.MeasurementNetwork.Id,
				Name = project.MeasurementNetwork.Name,
				Zone = project.MeasurementNetwork.Zone.Name
			}
		};

		return Result.Success(new ProjectResponse.Detail {
			Project = projectDto
		});
	}
}