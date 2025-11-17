using System;
using System.ComponentModel.DataAnnotations;

namespace Rise.Shared.Stations;

public static class MeasurementStationDto {
	public sealed class Index {
		public required Guid Id { get; set; }

		[Display(Name = "#")]
		public required string Code { get; set; }

		[Display(Name = "Status")]
		public required string Status { get; set; }

		[Display(Name = "Geïnstalleerd")]
		public required DateTime InstalledAt { get; set; }

		[Display(Name = "Verwijderd")]
		public DateTime? RemovedAt { get; set; }

		[Display(Name = "Locatie")]
		public string? Location { get; set; }

		[Display(Name = "Gemeente")]
		public string? City { get; set; }

		[Display(Name = "Projecten")]
		public int ProjectsCount { get; set; }
	}

	public sealed class Detail {
		public Guid Id { get; set; }
		public string Code { get; set; } = default!;
		public string Status { get; set; } = default!;
		public DateTime InstalledAt { get; set; }
		public DateTime? RemovedAt { get; set; }
		public string? Location { get; set; }
		public string? City { get; set; }
		public int ProjectsCount { get; set; }
	}
}