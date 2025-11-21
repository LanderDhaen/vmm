namespace Rise.Shared.Devices;

public static class ProjectResponse {
	public class Index {
		public IEnumerable<DevicesDto.Index> Devices { get; set; } = [];
		public int TotalCount { get; set; }
	}
}