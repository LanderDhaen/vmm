namespace Rise.Shared.Stations;

public static partial class MeasurementStationRequest {
	public sealed class UpdateMaintObjects {
		public Guid MeasurementStationId { get; set; }

		public List<Guid> MaintObjIds { get; set; } = new();
	}
}