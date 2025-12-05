using System.Text.Json;

using Microsoft.EntityFrameworkCore;

using Rise.Domain.Common;
using Rise.Domain.Locations;
using Rise.Domain.Projects;
using Rise.Domain.Stations;
using Rise.Persistence;
using Rise.Shared.Common;
using Rise.Shared.Projects;
using Rise.Shared.Stations;

using SharedStatus = Rise.Shared.Projects.Status;

namespace Rise.Services.Stations;

public class MeasurementStationService(ApplicationDbContext dbContext) : IMeasurementStationService {

	private static FilterSpec? ParseFilterSpec(object? rawValue) {
		if (rawValue == null) return null;

		try {
			if (rawValue is not JsonElement je)
				return new FilterSpec(null, rawValue.ToString());

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

	private static bool TryParseDateTimeSimple(string? raw, out DateTime d) {
		d = default;
		if (string.IsNullOrWhiteSpace(raw)) return false;
		raw = raw.Trim().Trim('"');
		if (DateTime.TryParseExact(raw, "yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.AssumeUniversal | System.Globalization.DateTimeStyles.AdjustToUniversal, out d))
			return true;
		return DateTime.TryParse(raw, out d);
	}

	public async Task<Result<MeasurementStationResponse.Index>> GetIndexAsync(
		QueryRequest.SkipTake request,
		CancellationToken ctx = default) {

		IQueryable<MeasurementStation> query = dbContext.MeasurementStations
			.AsQueryable()
			.Include(s => s.Location)
			.ThenInclude(l => l.Address)
			.Include(s => s.Projects)
			.AsNoTracking();

		if (!string.IsNullOrWhiteSpace(request.SearchTerm)) {
			var t = request.SearchTerm.Trim().ToLowerInvariant();
			query = query.Where(s =>
				s.Code.Contains(t) ||
				s.Location.Name.Contains(t) ||
				s.Location.Address.Addressline1.Contains(t) ||
				s.Location.Address.Addressline2.Contains(t) ||
				s.Location.Address.City.Contains(t) ||
				s.Location.Address.PostalCode.Contains(t)
			);
		}

		// filters
		if (request.Filters != null && request.Filters.Any()) {
			var temp = request.Filters
				.Select(kv => (Key: kv.Key, Spec: ParseFilterSpec(kv.Value)))
				.Where(x => x.Spec != null)
				.ToList();

			var parsed = temp.Select(x => (Key: x.Key!, Spec: x.Spec!)).ToList();

			foreach ((string keyOrig, FilterSpec spec) in parsed) {
				var key = keyOrig?.Trim();
				var raw = spec.Value;
				var opCandidate = spec.Operator;
				var opNorm = string.IsNullOrWhiteSpace(opCandidate) ? "Contains" : opCandidate;

				switch (key) {
				case "InstalledAt": {
						if (opNorm == "IsEmpty") {
							query = query.Where(s => s.Installation.InstalledAt == default);
						} else {
							if (TryParseDateTimeSimple(raw, out var d)) {
								if (opNorm == "GreaterThanOrEqual")
									query = query.Where(s => s.Installation.InstalledAt >= d);
								else if (opNorm == "LessThanOrEqual")
									query = query.Where(s => s.Installation.InstalledAt <= d);
								else
									query = query.Where(s => s.Installation.InstalledAt == d);
							}
						}
						break;
					}
				case "RemovedAt": {
						if (opNorm == "IsEmpty") {
							query = query.Where(s => s.Installation.RemovedAt == null);
						} else {
							if (TryParseDateTimeSimple(raw, out var d)) {
								if (opNorm == "GreaterThanOrEqual")
									query = query.Where(s => s.Installation.RemovedAt != null && s.Installation.RemovedAt >= d);
								else if (opNorm == "LessThanOrEqual")
									query = query.Where(s => s.Installation.RemovedAt != null && s.Installation.RemovedAt <= d);
								else
									query = query.Where(s => s.Installation.RemovedAt != null && s.Installation.RemovedAt == d);
							}
						}
						break;
					}
				case "Location": {
						var t = (raw ?? string.Empty).Trim().ToLowerInvariant();
						if (opNorm == "Equals" || opNorm == "Equal")
							query = query.Where(s => s.Location.Name.ToLower() == t);
						else
							query = query.Where(s => EF.Functions.Like(s.Location.Name.ToLower(), $"%{t}%"));
						break;
					}
				case "City": {
						var t = (raw ?? string.Empty).Trim().ToLowerInvariant();
						if (opNorm == "Equals" || opNorm == "Equal")
							query = query.Where(s => s.Location.Address.City.ToLower() == t);
						else
							query = query.Where(s => EF.Functions.Like(s.Location.Address.City.ToLower(), $"%{t}%"));
						break;
					}
				}
			}
		}

		int totalCount = await query.CountAsync(ctx);

		// Sorting
		if (!string.IsNullOrWhiteSpace(request.OrderBy)) {
			bool desc = request.OrderDescending;

			query = request.OrderBy switch {
				nameof(Installation.InstalledAt)
					=> desc
						? query.OrderByDescending(s => s.Installation.InstalledAt)
						: query.OrderBy(s => s.Installation.InstalledAt),

				nameof(Installation.RemovedAt)
					=> desc
						? query.OrderByDescending(s => s.Installation.RemovedAt)
						: query.OrderBy(s => s.Installation.RemovedAt),

				"Location"
					=> desc
						? query.OrderByDescending(s => s.Location.Name)
						: query.OrderBy(s => s.Location.Name),

				"City"
					=> desc
						? query.OrderByDescending(s => s.Location.Address.City)
						: query.OrderBy(s => s.Location.Address.City),

				nameof(MeasurementStationDto.Index.ProjectsCount)
					=> desc
						? query.OrderByDescending(s => s.Projects.Count())
						: query.OrderBy(s => s.Projects.Count()),

				_ => desc
					? query.OrderByDescending(s => EF.Property<object>(s, request.OrderBy!))
					: query.OrderBy(s => EF.Property<object>(s, request.OrderBy!))
			};
		} else {
			query = query.OrderBy(s => s.Id);
		}

		// Paginating
		query = query.Skip(request.Skip).Take(request.Take);

		// Selecting
		List<MeasurementStationDto.Index> stations = await query
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

		return Result.Success(new MeasurementStationResponse.Index {
			Stations = stations,
			TotalCount = totalCount
		});
	}

	public async Task<Result<MeasurementStationResponse.Detail>> GetDetailAsync(
		Guid stationId,
		CancellationToken ct = default) {
		MeasurementStation? station = await dbContext.MeasurementStations
			.AsNoTracking()
			.Include(s => s.Location)
			.Include(s => s.Projects)
			.SingleOrDefaultAsync(s => s.Id == stationId, ct);

		if (station is null) {
			return Result.NotFound();
		}

		MeasurementStationDto.Detail dto = new() {
			Id = station.Id,
			Code = station.Code,
			InstalledAt = station.Installation.InstalledAt,
			RemovedAt = station.Installation.RemovedAt,
			Location = station.Location.Name,
			City = station.Location.Address.City,
			ProjectsCount = station.Projects.Count()
		};

		return Result.Success(new MeasurementStationResponse.Detail { Station = dto });
	}

	public async Task<Result<MeasurementStationResponse.History>> GetHistoryAsync(Guid stationId,
		CancellationToken ct = default) {

		IQueryable<MeasurementStation> query = dbContext.MeasurementStations.AsQueryable().AsNoTracking();

		MeasurementStationDto.History? measurementStation = await query
			.Where(s => s.Id == stationId)
			.Select(s => new MeasurementStationDto.History {
				Id = s.Id,
				Code = s.Code,
				InstalledAt = s.Installation.InstalledAt,
				RemovedAt = s.Installation.RemovedAt,
				Location = s.Location.Name,
				City = s.Location.Address.City,
				MeasurementStationHistories = s.MeasurementStationHistories
					.OrderByDescending(h => h.ValidityRange.ValidTo)
					.Select(h => new MeasurementStationHistoryDto.Index {
						Id = h.Id,
						InstalledAt = h.Installation.InstalledAt,
						RemovedAt = h.Installation.RemovedAt,
						Location = h.Location.Name,
						City = h.Location.Address.City,
						ValidFrom = h.ValidityRange.ValidFrom,
						ValidTo = h.ValidityRange.ValidTo,
					}).ToList()
			})
			.FirstOrDefaultAsync(ct);

		if (measurementStation is null) {
			return Result.NotFound($"Measurement station with Id '{stationId}' was not found");
		}

		return Result.Success(new MeasurementStationResponse.History {
			MeasurementStation = measurementStation
		});
	}

	public async Task<Result<MeasurementStationResponse.Create>> CreateAsync(MeasurementStationRequest.Create request,
		CancellationToken ct = default) {
		Location location;
		if (request.IsNewLocation) {
			location = new Location(
				request.Location!.Name,
				new Address(
					request.Location.Addressline1,
					request.Location.Addressline2,
					request.Location.City,
					request.Location.PostalCode,
					request.Location.CountryIso
				),
				new Coordinate(request.Location.X, request.Location.Y)
			);
		} else {
			location = await dbContext.Set<Location>()
				.SingleOrDefaultAsync(l => l.Id == request.LocationId, ct);
			if (location is null) {
				return Result.NotFound("Locatie niet gevonden.");
			}
		}

		DateTime installedAt = request.InstalledAt ?? DateTime.UtcNow;

		MeasurementStation station = new(
			request.Code,
			location,
			new Installation(installedAt)
		);

		dbContext.MeasurementStations.Add(station);
		await dbContext.SaveChangesAsync(ct);

		return Result.Success(new MeasurementStationResponse.Create {
			MeasurementStationId = station.Id
		});
	}

	public Task<Result> EditAsync(
		MeasurementStationRequest.Edit request,
		CancellationToken ct = default) {
		return UpdateAsync(request.StationId, request, ct);
	}

	public async Task<Result<List<ProjectDto.Index>>> GetProjectsAsync(
		Guid stationId,
		CancellationToken ct = default) {
		bool exists = await dbContext.MeasurementStations
			.AsNoTracking()
			.AnyAsync(s => s.Id == stationId, ct);

		if (!exists) {
			return Result.NotFound("Meetstation niet gevonden.");
		}

		List<ProjectDto.Index> projects = await dbContext.Projects
			.AsNoTracking()
			.Where(p => p.Stations.Any(s => s.Id == stationId))
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
			.ToListAsync(ct);

		return Result.Success(projects);
	}

	public async Task<Result> UpdateAsync(
		Guid id,
		MeasurementStationRequest.Edit request,
		CancellationToken ct = default) {

		MeasurementStation? station = await dbContext.MeasurementStations
			.Include(s => s.Location)
			.Include(s => s.Projects)
			.SingleOrDefaultAsync(s => s.Id == id, ct);

		if (station is null) {
			return Result.NotFound();
		}

		DateTime installedAt = request.InstalledAt ?? station.Installation.InstalledAt;
		station.UpdateInstallation(new Installation(installedAt, request.RemovedAt));

		if (station.Location.Id != request.LocationId) {
			Location? newLocation = await dbContext.Set<Location>()
				.SingleOrDefaultAsync(l => l.Id == request.LocationId, ct);
			if (newLocation is null) {
				return Result.NotFound();
			}

			station.MoveTo(newLocation);
		}

		Project? newProject = await dbContext.Set<Project>()
			.SingleOrDefaultAsync(p => p.Id == request.ProjectId, ct);
		if (newProject is null) {
			return Result.NotFound();
		}

		station.Projects.Clear();
		station.Projects.Add(newProject);

		await dbContext.SaveChangesAsync(ct);
		return Result.Success();
	}

	public async Task<Result> UpdateProjectsAsync(MeasurementStationRequest.UpdateProjects req,
		CancellationToken ctx = default) {

		MeasurementStation? measurementStation = await dbContext.MeasurementStations
			.Include(p => p.Projects)
			.SingleOrDefaultAsync(x => x.Id == req.MeasurementStationId, ctx);

		if (measurementStation is null) {
			return Result.NotFound($"Meetstation met id '{req.MeasurementStationId}' is niet gevonden.");
		}

		List<Project> projects = await dbContext.Projects
			.Where(p => req.ProjectIds.Contains(p.Id))
			.ToListAsync(ctx);

		if (projects.Count != req.ProjectIds.Count()) {
			return Result.Error("Een of meerdere projecten zijn niet gevonden.");
		}

		measurementStation.UpdateProjectList(projects);

		await dbContext.SaveChangesAsync(ctx);

		return Result.Success();
	}
};