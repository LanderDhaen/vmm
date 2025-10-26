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

		if (!string.IsNullOrWhiteSpace(request.OrderBy)) {
			query = query.OrderBy(p => EF.Property<object>(p, request.OrderBy));
		} else {
			query = query.OrderBy(p => p.Name);
		}

		List<ProjectDto.Index> projects = await query.AsNoTracking()
			.Skip(request.Skip)
			.Take(request.Take)
			.Select(p => new ProjectDto.Index {
				Id = p.Id,
				Name = p.Name,
				Code = p.Code,
				DateRange = new DateRangeDto.Index {
					StartDate = p.DateRange.StartDate,
					EndDate = p.DateRange.EndDate
				},
				Location = new LocationDto.Index {
					Id = p.Location.Id,
					Name = p.Location.Name,
					Address = new AddressDto.Index {
						AddressLine1 = p.Location.Address.Addressline1,
						AddressLine2 = p.Location.Address.Addressline2,
						City = p.Location.Address.City,
						PostalCode = p.Location.Address.PostalCode,
						CountryIso = p.Location.Address.CountryIso
					},
					Coordinate = new CoordinateDto.Index {
						X = p.Location.Coordinate.X,
						Y = p.Location.Coordinate.Y
					}
				}
			})
			.ToListAsync(ctx);

		return Result.Success(new ProjectResponse.Index {
			Projects = projects,
			TotalCount = totalCount
		});
	}
	
	public async Task<Result<ProjectResponse.Detail>> GetDetailAsync(Guid projectId, CancellationToken ctx = default) {
	    ProjectDto.Detail? project = await dbContext.Projects.AsNoTracking()
	        .Where(p => p.Id == projectId)
	        .Select(p => new ProjectDto.Detail {
	            Id = p.Id,
	            Name = p.Name,
	            Code = p.Code,
	            DateRange = new DateRangeDto.Index {
	                StartDate = p.DateRange.StartDate,
	                EndDate = p.DateRange.EndDate
	            },
	            Location = new LocationDto.Index {
	                Id = p.Location.Id,
	                Name = p.Location.Name,
	                Address = new AddressDto.Index {
	                    AddressLine1 = p.Location.Address.Addressline1,
	                    AddressLine2 = p.Location.Address.Addressline2,
	                    City = p.Location.Address.City,
	                    PostalCode = p.Location.Address.PostalCode,
	                    CountryIso = p.Location.Address.CountryIso
	                },
	                Coordinate = new CoordinateDto.Index {
	                    X = p.Location.Coordinate.X,
	                    Y = p.Location.Coordinate.Y
	                }
	            }
	        })
	        .SingleOrDefaultAsync(ctx);
	
	    if (project is null) {
	        return Result<ProjectResponse.Detail>.NotFound($"Project with Id '{projectId}' was not found.");
	    }
	
	    return Result.Success(new ProjectResponse.Detail { Project = project });
	}
}