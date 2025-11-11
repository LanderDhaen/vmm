using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.Common;
using Rise.Shared.Networks;
using Rise.Shared.Projects;

namespace Rise.Client.Projects;

public partial class Create {
	private readonly ProjectRequest.Create.Validator _validator = new();
	private IEnumerable<MeasurementNetworkDto.Index>? _networks;
	private Result<ProjectResponse.Create>? _result;
	private MudForm form;
	[Inject] private ISnackbar Snackbar { get; set; }
	[Inject] public required IProjectService ProjectService { get; set; }
	[Inject] public required INetworkService NetworkService { get; set; }
	[Inject] public required NavigationManager NavigationManager { get; set; }
	private ProjectRequest.Create Model { get; } = new();

	protected override async Task OnInitializedAsync() {
		Result<NetworkResponse.Index> result = await NetworkService.GetIndexAsync(new QueryRequest.SkipTake());
		_networks = result.Value.measurementsNetworks;
	}

	private async Task CreateProjectAsync() {
		await form.Validate();

		if (form.IsValid) {
			_result = await ProjectService.CreateAsync(Model);
			if (_result.IsSuccess) {
				Snackbar.Add("Project succesvol aangemaakt!", Severity.Success);
				await Task.Delay(2000);
				NavigationManager.NavigateTo("/projects");
			} else {
				Snackbar.Add(string.Join("\n", _result.Errors), Severity.Error);
			}
		}
	}

	private async Task<IEnumerable<Guid?>> Search(string value, CancellationToken token) {
		if (string.IsNullOrEmpty(value)) {
			return _networks.Select(x => (Guid?)x.Id);
		}

		return _networks
			.Where(x => x.Name.Contains(value, StringComparison.InvariantCultureIgnoreCase))
			.Select(x => (Guid?)x.Id);
	}
}