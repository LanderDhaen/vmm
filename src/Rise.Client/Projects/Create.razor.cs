using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Services;
using Rise.Shared.Common;
using Rise.Shared.Networks;
using Rise.Shared.Projects;

namespace Rise.Client.Projects;

public partial class Create {
	private readonly ProjectRequest.Create.Validator validator = new();
	private Result<ProjectResponse.Create>? _result;
	private MudForm form;
	private IEnumerable<MeasurementNetworkDto.Index>? networks;
	[Inject] private ISnackbar Snackbar { get; set; }
	[Inject] public required IProjectService ProjectService { get; set; }
	[Inject] public required INetworkService NetworkService { get; set; }
	[Inject] public required NavigationManager NavigationManager { get; set; }
	[Inject] public required HeaderService HeaderService { get; set; }
	[Inject] public required BreadcrumbService BreadcrumbService { get; set; }

	private ProjectRequest.Create Model { get; } = new();

	protected override async Task OnInitializedAsync() {
		QueryRequest.SkipTake request = new() {
			Skip = 0,
			Take = 50,
			OrderBy = "Id"
		};

		Result<NetworkResponse.Index> result = await NetworkService.GetIndexAsync(request);
		networks = result.Value.measurementsNetworks;

		BreadcrumbService.SetBreadcrumbs(
			new BreadcrumbItem("Projecten", "/projects"),
			new BreadcrumbItem("Voeg toe", "/create", true)
		);

		HeaderService.Set(
			"Voeg toe",
			Icons.Material.Filled.ArrowBack,
			() => {
				NavigationManager.NavigateTo("/projects/");
				return Task.CompletedTask;
			}
		);
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
			return networks.Select(x => (Guid?)x.Id);
		}

		return networks
			.Where(x => x.Name.Contains(value, StringComparison.InvariantCultureIgnoreCase))
			.Select(x => (Guid?)x.Id);
	}
}