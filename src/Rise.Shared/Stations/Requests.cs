using System;

namespace Rise.Shared.Stations;

// CREATE
public class CreateMeasurementStationRequest
{
	public string Code { get; set; } = default!;
	public string Status { get; set; } = default!;
	public DateTime? InstalledAt { get; set; }
	public Guid LocationId { get; set; }
}

// UPDATE
public class UpdateMeasurementStationRequest
{
	public string Code { get; set; } = default!;
	public string Status { get; set; } = default!;
	public DateTime? InstalledAt { get; set; }
	public DateTime? RemovedAt { get; set; }
	public Guid LocationId { get; set; }
}

// LISTING (paging + search)
public class MeasurementStationListRequest
{
	public int Page { get; set; } = 1;         // 1-based
	public int PageSize { get; set; } = 10;
	public string? Search { get; set; }        // filters Code/Status
}

