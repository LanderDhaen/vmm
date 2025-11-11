using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Shared.Common;
using Rise.Shared.Projects;

using Serilog;

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
		// Use centralized fetch to avoid duplicating error handling
		int skip = state.Page * state.PageSize;
		int take = state.PageSize;
		// OrderBy can be adapted to use state sorting if available; keep "Id" to match previous behavior
		var result = await FetchProjectsAsync(skip, take, "Id");

		if (result?.Value is not null) {
			return new GridData<ProjectDto.Index> {
				TotalItems = result.Value.TotalCount,
				Items = result.Value.Projects
			};
		}

		// On error or empty result return an empty dataset so grid can render safely
		return new GridData<ProjectDto.Index> {
			TotalItems = 0,
			Items = Array.Empty<ProjectDto.Index>()
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

	// --- Centralized fetch used by both grid and mobile -----------------

	/// <summary>
	/// Centralized fetch that builds the request, calls the service and handles logging/errors.
	/// Returns null on exception so callers can handle fallback behaviour.
	/// </summary>
	private async Task<Result<ProjectResponse.Index>?> FetchProjectsAsync(int skip, int take, string? orderBy = null) {
		try {
			var req = BuildRequest(skip, take, orderBy);
			var result = await ProjectService.GetIndexAsync(req);
			return result;
		} catch (Exception ex) {
			// Single place for logging errors for all callers
			Log.Error(ex, "FetchProjectsAsync failed (skip={Skip}, take={Take}, orderBy={OrderBy})", skip, take, orderBy);
			return null;
		}
	}

	// --- Mobile data (cards) --------------------------------------------

	// Default parameter values make fields unnecessary; call without parameters for page 0 / size 10
	private async Task FetchMobileItemsAsync(int page = 0, int pageSize = 10, string? orderBy = null) {
		var response = await FetchProjectsAsync(page * pageSize, pageSize, orderBy);

		if (response?.Value is not null) {
			_mobileItems = response.Value.Projects?.ToList() ?? new List<ProjectDto.Index>();
			_mobileTotal = response.Value.TotalCount;
		} else {
			Log.Warning("FetchMobileItemsAsync: service returned null or empty result");
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