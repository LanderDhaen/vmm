using Microsoft.AspNetCore.Components;

using Rise.Shared.Common;
using Rise.Shared.Networks;
using Rise.Shared.Projects;

namespace Rise.Client.Components.Projects;

public partial class CreateProjectForm {
	private IEnumerable<MeasurementNetworkDto.Index>? _networks;
	private CancellationTokenSource _cancellationTokenSource = new();
	private string _searchTerm = string.Empty;
	
	[Parameter] public required ProjectRequest.Create Model { get; set; } = new();
	
	[Inject] public required INetworkService NetworkService { get; set; }

	protected override async Task OnInitializedAsync() {
		await RefreshMeasurementNetworks();
	}
	
	private async Task RefreshMeasurementNetworks() {
		await _cancellationTokenSource.CancelAsync();
		_cancellationTokenSource.Dispose();
		_cancellationTokenSource = new CancellationTokenSource();

		QueryRequest.SkipTake request = new() {
			SearchTerm = _searchTerm
		};

		Result<NetworkResponse.Index> result = await NetworkService.GetIndexAsync(request, _cancellationTokenSource.Token);

		foreach (MeasurementNetworkDto.Index valueMeasurementsNetwork in result.Value.measurementsNetworks)
		{
			Console.WriteLine(valueMeasurementsNetwork.Name);
		}
		
		if (result.IsSuccess) {
			_networks = result.Value.measurementsNetworks;
		}
	}

	private async Task<IEnumerable<Guid?>> Search(string value, CancellationToken token) {
		_searchTerm = value;
		await RefreshMeasurementNetworks();

		return _networks?.Select(l => (Guid?)l.Id) ?? [];
	}
}