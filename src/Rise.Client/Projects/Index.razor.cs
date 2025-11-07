using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.Common;
using Rise.Shared.Projects;

namespace Rise.Client.Projects;

public partial class Index {
	[Inject] public required IProjectService ProjectService { get; set; }
	[Inject] public required NavigationManager NavigationManager { get; set; }

	// Mobile list state (without separate page/pageSize fields)
	private List<ProjectDto.Index>? _mobileItems;
	private int _mobileTotal;

	protected override async Task OnInitializedAsync() {
		await base.OnInitializedAsync();
		await FetchMobileItemsAsync(); // uses default page = 0, pageSize = 10
	}

	private async Task<GridData<ProjectDto.Index>> GetProjectsAsync(GridState<ProjectDto.Index> state) {
		QueryRequest.SkipTake request = new() {
			Skip = state.Page * state.PageSize,
			Take = state.PageSize,
			OrderBy = "Id"
		};

		Result<ProjectResponse.Index> result = await ProjectService.GetIndexAsync(request);

		return new GridData<ProjectDto.Index> {
			TotalItems = result.Value.TotalCount,
			Items = result.Value.Projects
		};
	}

	private void OnRowClick(DataGridRowClickEventArgs<ProjectDto.Index> args) {
		NavigationManager.NavigateTo($"/projects/{args.Item.Id}");
	}

	private static QueryRequest.SkipTake BuildRequest(int skip, int take, string? orderBy)
		=> new() {
			Skip = skip,
			Take = take,
			OrderBy = orderBy,
		};

	// --- Mobile data (cards) --------------------------------------------

	// Default parameter values make fields unnecessary; call without parameters for page 0 / size 10
	private async Task FetchMobileItemsAsync(int page = 0, int pageSize = 10, string? orderBy = null) {
		try {
			var req = BuildRequest(page * pageSize, pageSize, orderBy);
			var result = await ProjectService.GetIndexAsync(req);

			if (result?.Value is not null) {
				_mobileItems = result.Value.Projects?.ToList() ?? new List<ProjectDto.Index>();
				_mobileTotal = result.Value.TotalCount;
			} else {
				Log.Warning("FetchMobileItemsAsync: service returned null or empty result");
				_mobileItems = new List<ProjectDto.Index>();
				_mobileTotal = 0;
			}
		} catch (Exception ex) {
			Log.Error(ex, "Failed to fetch mobile project items");
			_mobileItems = new List<ProjectDto.Index>();
			_mobileTotal = 0;
		}

		await InvokeAsync(StateHasChanged);
	}

	// NAVIGATION FOR CARDS (click/tap)
	private void NavigateToProject(object? id) {
		if (id is null) return;
		NavigationManager.NavigateTo($"/projects/{id}");
	}
}