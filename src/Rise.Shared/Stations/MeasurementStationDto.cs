using Rise.Shared.Locations;

namespace Rise.Shared.Stations;

public static class MeasurementStationDto {
	public sealed class Index {
		public Guid Id { get; set; }

		public string Code { get; set; } = default!;
		public string Status { get; set; } = default!;

		public DateTime InstalledAt { get; set; }
		public DateTime? RemovedAt { get; set; }

		public LocationDto.Index Location { get; set; } = default!;

		public string ProjectName { get; set; }
		public string ProjectCode { get; set; }
	}
}