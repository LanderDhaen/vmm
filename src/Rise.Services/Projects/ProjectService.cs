using Microsoft.EntityFrameworkCore;

using Rise.Domain.Projects;
using Rise.Persistence;
using Rise.Services.Identity;
using Rise.Shared.Common;
using Rise.Shared.Identity;
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

		int totalCount = await query.CountAsync();

		List<ProjectDto.Index> projects = await query.AsNoTracking()
			.Skip(request.Skip)
			.Take(request.Take)
			.Select(p => new ProjectDto.Index {
				Id = p.Id,
				Name = p.Name,
				Code = p.Code,
				StartDate = p.StartDate,
				EndDate = p.EndDate
			})
			.ToListAsync(ctx);

		return Result.Success(new ProjectResponse.Index {
			Projects = projects,
			TotalCount = totalCount
		});
	}
}