using Microsoft.EntityFrameworkCore;

using Rise.Domain.Networks;
using Rise.Domain.Projects;
using Rise.Domain.Stations;
using Rise.Persistence;
using Rise.Shared.Common;
using Rise.Shared.Networks;
using Rise.Shared.Projects;
using Rise.Shared.Stations;

using SharedProjectModificationType = Rise.Shared.Projects.ProjectModificationType;
using SharedStatus = Rise.Shared.Projects.Status;
using Status = Rise.Domain.Projects.Status;

namespace Rise.Services.Projects;

/// <summary>
///     Service for projects.
/// </summary>
/// <param name="dbContext"></param>
public class ProjectService(ApplicationDbContext dbContext)
	: IProjectService {
	public async Task<Result<ProjectResponse.Create>> CreateAsync(ProjectRequest.Create request,
		CancellationToken ctx = default) {
		if (await dbContext.Projects.AnyAsync(x => x.Code == request.Code, ctx)) {
			Log.Warning("Project met code '{Code}' bestaat al.", request.Code);
			return Result.Conflict($"Project met code '{request.Code}' bestaat al.");
		}

		DateOnly startdate = DateOnly.FromDateTime(request.StartDate!.Value);
		MeasurementNetwork? measurementNetwork = await dbContext.MeasurementNetworks
			.FirstOrDefaultAsync(x => x.Id == request.MeasurementNetworkId, ctx);

		if (measurementNetwork is null) {
			return Result.NotFound("Meetnet niet gevonden.");
		}

		Project p = new(request.Name, request.Code, new DateRange(startdate), measurementNetwork);

		if (request.MeasurementStationId != Guid.Empty) {
			MeasurementStation? measurementStation = await dbContext.MeasurementStations
				.SingleOrDefaultAsync(x => x.Id == request.MeasurementStationId, ctx);

			if (measurementStation is null) {
				return Result.NotFound("Meetstation niet gevonden.");
			}

			p.UpdateMeasurementStationList(new List<MeasurementStation> { measurementStation });
		}

		dbContext.Projects.Add(p);

		await dbContext.SaveChangesAsync(ctx);

		return Result.Created(new ProjectResponse.Create {
			ProjectId = p.Id
		});
	}

	public async Task<Result> EditAsync(ProjectRequest.Edit req, CancellationToken ctx = default) {
		Project? project = await dbContext.Projects
			.Include(x => x.MeasurementNetwork)
			.ThenInclude(x => x.Zone)
			.Include(x => x.DateRange)
			.SingleOrDefaultAsync(x => x.Id == req.ProjectId, ctx);

		if (project is null) {
			return Result.NotFound($"Project met Id '{req.ProjectId}' was niet gevonden.");
		}

		if (await dbContext.Projects.AnyAsync(x => x.Code == req.Code && x.Id != req.ProjectId, ctx)) {
			Log.Warning("Project met code '{Code}' bestaat al.", req.Code);
			return Result.Conflict($"Project met code '{req.Code}' bestaat al.");
		}

		DateOnly startdate = DateOnly.FromDateTime(req.StartDate!.Value);
		DateOnly? enddate = req.EndDate.HasValue ? DateOnly.FromDateTime(req.EndDate.Value) : null;

		project.Edit(req.Name, req.Code, new DateRange(startdate, enddate));

		await dbContext.SaveChangesAsync(ctx);

		return Result.Success();
	}

	public async Task<Result> UpdateMeasurementNetworkAsync(
		ProjectRequest.UpdateMeasurementNetwork req,
		CancellationToken ctx = default) {
		Project? project = await dbContext.Projects
			.Include(x => x.MeasurementNetwork)
			.ThenInclude(x => x.Zone)
			.Include(x => x.DateRange)
			.SingleOrDefaultAsync(x => x.Id == req.ProjectId, ctx);

		if (project is null) {
			return Result.NotFound($"Project met Id '{req.ProjectId}' was niet gevonden.");
		}

		MeasurementNetwork? measurementNetwork = await dbContext.MeasurementNetworks
			.FirstOrDefaultAsync(x => x.Id == req.MeasurementNetworkId, ctx);

		if (measurementNetwork is null) {
			return Result.NotFound("Meetnet niet gevonden.");
		}

		project.UpdateMeasurementNetwork(measurementNetwork);

		await dbContext.SaveChangesAsync(ctx);

		return Result.Success();
	}

	public async Task<Result<ProjectResponse.Index>> GetIndexAsync(QueryRequest.SkipTake req,
		CancellationToken ctx = default) {
		IQueryable<Project> query = dbContext.Projects.AsQueryable().AsNoTracking();

		// Only get active projects
		if (req.Filters.TryGetValue("Status", out object? statusObj) && statusObj is string and "Active") {
			query = query.Where(p => p.Status == Status.Actief);
		}

		// Filtering (MSSQL handles casing)

		if (!string.IsNullOrWhiteSpace(req.SearchTerm)) {
			query = query.Where(p =>
				p.Name.Contains(req.SearchTerm) ||
				p.Code.Contains(req.SearchTerm) ||
				p.MeasurementNetwork.Name.Contains(req.SearchTerm) ||
				p.MeasurementNetwork.Zone.Name.Contains(req.SearchTerm)
			);
		}

		int totalCount = await query.CountAsync(ctx);

		// Sorting

		if (!string.IsNullOrWhiteSpace(req.OrderBy)) {
			bool desc = req.OrderDescending;
			switch (req.OrderBy) {
			case nameof(DateRange.StartDate):
				query = desc
					? query.OrderByDescending(p => p.DateRange.StartDate)
					: query.OrderBy(p => p.DateRange.StartDate);
				break;
			case nameof(DateRange.EndDate):
				query = desc
					? query.OrderByDescending(p => p.DateRange.EndDate)
					: query.OrderBy(p => p.DateRange.EndDate);
				break;
			case nameof(MeasurementNetwork):
				query = desc
					? query.OrderByDescending(p => p.MeasurementNetwork.Name)
					: query.OrderBy(p => p.MeasurementNetwork.Name);
				break;
			case nameof(Zone):
				query = desc
					? query.OrderByDescending(p => p.MeasurementNetwork.Zone.Name)
					: query.OrderBy(p => p.MeasurementNetwork.Zone.Name);
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

		// Paginating

		query = query
			.Skip(req.Skip)
			.Take(req.Take);

		// Selecting

		List<ProjectDto.Index> projects = await query
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
			.Include(p => p.DateRange)
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

	public async Task<Result<ProjectResponse.History>>
		GetHistoryAsync(Guid projectId, CancellationToken ctx = default) {

		Project? project = await dbContext.Projects
			.AsNoTracking()
			.Include(p => p.DateRange)
			.Include(p => p.MeasurementNetwork)
			.ThenInclude(mn => mn.Zone)
			.Include(p => p.ProjectHistories)
			.ThenInclude(h => h.DateRange)
			.Include(p => p.ProjectHistories)
			.ThenInclude(h => h.MeasurementNetwork)
			.ThenInclude(mn => mn.Zone)
			.Include(p => p.ProjectHistories.OrderByDescending(h => h.ValidityRange.ValidFrom))
			.SingleOrDefaultAsync(p => p.Id == projectId, ctx);

		if (project is null) {
			return Result.NotFound($"Project with Id '{projectId}' was not found");
		}

		List<ProjectDto.Modification> modifications = [];

		foreach (ProjectModification m in project.GetModifications()) {
			modifications.Add(new ProjectDto.Modification {
				ProjectModificationType = (SharedProjectModificationType)m.MeasurementStationModificationType,
				ModifiedAt = m.ModifiedAt,
				PreviousValue = m.PreviousValue,
				CurrentValue = m.CurrentValue
			});
		}

		return Result.Success(new ProjectResponse.History {
			Modifications = modifications
		});
	}


	public async Task<Result> UpdateStatusAsync(ProjectRequest.UpdateStatus req, CancellationToken ctx = default) {
		Project? project = await dbContext.Projects
			.Include(p => p.MeasurementNetwork)
			.ThenInclude(p => p.Zone)
			.Include(p => p.ProjectHistories)
			.SingleOrDefaultAsync(x => x.Id == req.ProjectId, ctx);

		if (project is null) {
			return Result.NotFound($"Project with Id '{req.ProjectId}' was not found.");
		}

		project.UpdateStatus((Status)req.NewStatus);

		await dbContext.SaveChangesAsync(ctx);

		return Result.Success();
	}

	public async Task<Result<ProjectResponse.MeasurementStations>> GetStationsAsync(
		Guid projectId,
		CancellationToken ctx = default) {
		bool exists = await dbContext.Projects
			.AsNoTracking()
			.AnyAsync(p => p.Id == projectId, ctx);

		if (!exists) {
			return Result.NotFound($"Project met Id '{projectId}' was niet gevonden.");
		}

		IQueryable<MeasurementStation> query = dbContext.MeasurementStations
			.AsNoTracking()
			.AsQueryable();

		List<MeasurementStationDto.Index> stations = await query
			.Where(s => s.Projects.Any(p => p.Id == projectId))
			.Select(s => new MeasurementStationDto.Index {
				Id = s.Id,
				Code = s.Code,
				InstalledAt = s.Installation.InstalledAt,
				RemovedAt = s.Installation.RemovedAt,
				Location = s.Location.Name,
				City = s.Location.Address.City,
				ProjectsCount = s.Projects.Count()
			})
			.ToListAsync(ctx);

		return Result.Success(new ProjectResponse.MeasurementStations {
			MeasurementStationsFromProject = stations
		});
	}

	public async Task<Result> UpdateMeasurementStationsAsync(ProjectRequest.UpdateMeasurementStations req,
		CancellationToken ctx = default) {
		Project? project = await dbContext.Projects
			.Include(p => p.Stations)
			.SingleOrDefaultAsync(x => x.Id == req.ProjectId, ctx);

		if (project is null) {
			return Result.NotFound($"Project met id '{req.ProjectId}' is niet gevonden.");
		}

		List<MeasurementStation> measurementStations = await dbContext.MeasurementStations
			.Where(ms => req.MeasurementStationIds.Contains(ms.Id))
			.ToListAsync(ctx);

		if (measurementStations.Count != req.MeasurementStationIds.Count) {
			return Result.Error("Een of meerdere meetstations zijn niet gevonden.");
		}

		project.UpdateMeasurementStationList(measurementStations);

		await dbContext.SaveChangesAsync(ctx);

		return Result.Success();
	}
}