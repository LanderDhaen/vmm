using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using FastEndpoints;

using Microsoft.EntityFrameworkCore;

using Rise.Persistence;
using Rise.Shared.Common;
using Rise.Shared.Identity;

namespace Rise.Server.Endpoints.WorkOrders {
	// Request DTO voor querystring: /api/workorders/technicians?query=jan
	public class TechnicianQuery {
		public string? Query { get; set; }
	}

	/// <summary>
	/// Endpoint om server-side suggesties voor technician-namen te geven.
	/// </summary>
	public class Technicians : Endpoint<TechnicianQuery, Result<List<string>>> {
		private readonly ApplicationDbContext _db;

		public Technicians(ApplicationDbContext db) => _db = db;

		public override void Configure() {
			Get("/api/workorders/technicians");
			Roles(AppRoles.Administrator);
			AllowAnonymous();
		}

		public override async Task<Result<List<string>>> ExecuteAsync(TechnicianQuery req, CancellationToken ct) {
			var q = (req?.Query ?? string.Empty).Trim().ToLowerInvariant();

			var techniciansQuery = _db.WorkOrders
				.Where(w => w.Technician != null)
				.Select(w => new { w.Technician!.FirstName, w.Technician!.LastName })
				.Distinct();

			if (!string.IsNullOrEmpty(q)) {
				techniciansQuery = techniciansQuery.Where(t =>
					(t.FirstName + " " + t.LastName).ToLower().Contains(q) ||
					(t.LastName + " " + t.FirstName).ToLower().Contains(q)
				);
			}

			var names = await techniciansQuery
				.Select(t => t.FirstName + " " + t.LastName)
				.OrderBy(n => n)
				.Take(50)
				.ToListAsync(ct);

			return Result.Success(names);
		}
	}
}