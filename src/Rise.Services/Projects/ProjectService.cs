using Microsoft.EntityFrameworkCore;

using Rise.Domain.Projects;
using Rise.Persistence;
using Rise.Services.Identity;
using Rise.Shared.Common;
using Rise.Shared.Identity;
using Rise.Shared.Locations;
using Rise.Shared.Projects;

namespace Rise.Services.Projects;

/// <summary>
///     Service for projects. Note the use of <see cref="ISessionContextProvider" /> to get the current user in this layer
///     of the application.
/// </summary>
/// <param name="dbContext"></param>
/// <param name="sessionContextProvider"></param>
public class ProjectService(ApplicationDbContext dbContext, ISessionContextProvider sessionContextProvider) : IProjectService {
	public async Task<Result<ProjectResponse.Create>> CreateAsync(ProjectRequest.Create request, CancellationToken ctx = default) {
		if (await dbContext.Projects.AnyAsync(x => x.Code == request.Code)) {
			Log.Warning("Project met code '{Code}' bestaat al.", request.Code);
			return Result.Conflict($"Project met code '{request.Code}' bestaat al.");
		}

		var startdate = DateOnly.FromDateTime(request.StartDate!.Value);
		var measurementNetwork = await dbContext.MeasurementNetworks
			.FirstOrDefaultAsync(x => x.Id == request.MeasurementNetworkId);

		if (measurementNetwork == null) {
			return Result.Conflict("Meetnet niet gevonden.");
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
			return Result.NotFound($"Project with Id '{req.ProjectId}' was not found.");
		}

		// User must be logged in to perform an edit.
		Technician? loggedInTechnician = await dbContext.Technicians
			.SingleOrDefaultAsync(x => x.AccountId == sessionContextProvider.User!.GetUserId(), ctx);

		if (loggedInTechnician is null) {
			return Result.Unauthorized("You are not authorized to edit this project.");
		}

		project.Edit(req.Name);

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
		
		List<ProjectDto.Index> projects = await Queryable
			.Select(page, p => new ProjectDto.Index
			{
				Id = p.Id,
				Name = p.Name,
				Code = p.Code,
				StartDate = p.DateRange.StartDate,
				EndDate = p.DateRange.EndDate,	
				MeasurementNetwork = p.MeasurementNetwork.Name,
				Zone = p.MeasurementNetwork.Zone.Name,	
			})
			.ToListAsync(ctx);

		return Result.Success(new ProjectResponse.Index
		{
			Projects = projects,
			TotalCount = totalCount
		});
	}
}