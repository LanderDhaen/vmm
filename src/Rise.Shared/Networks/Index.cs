namespace Rise.Shared.Networks;

/// <summary>
///     Represents the response structure for measurement network-related operations.
/// </summary>
public static class NetworkResponse {
	public class Index {
		public IEnumerable<MeasurementNetworkDto.Index> measurementsNetworks { get; set; } = [];
	}
}