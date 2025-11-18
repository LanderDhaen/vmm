using System.Linq;

using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Services;
using Rise.Shared.Common;
using Rise.Shared.Projects;

using Serilog;

namespace Rise.Client.Projects;

public partial class Index {
	// Mobile list state (without separate page/pageSize fields)
	private List<ProjectDto.Index>? _mobileItems;
	private int _mobileTotal;

	// 🔹 Controls visibility of the bottom sort sheet on mobile
	private bool _mobileSortOptionsOpen;

	// 🔹 Which column we want to sort by in the mobile view
	private enum MobileProjectSortColumn {
		Code,
		Name,
		StartDate,
		EndDate,
		MeasurementNetwork,
		Zone,
		Status
	}

	[Inject] public required IProjectService ProjectService { get; set; }
	[Inject] public required NavigationManager NavigationManager { get; set; }
	[Inject] public required BreadcrumbService BreadcrumbService { get; set; }
	[Inject] public required HeaderService HeaderService { get; set; }

	protected override async Task OnInitializedAsync() {
		await base.OnInitializedAsync();
		await FetchMobileItemsAsync(); // uses default page = 0, pageSize = 10
		BreadcrumbService.SetBreadcrumbs(
			new BreadcrumbItem("Home", "/"),
			new BreadcrumbItem("Projecten", null, true)
		);
		// set header
		HeaderService.Set("Projecten");
	}

	private async Task<GridData<ProjectDto.Index>> GetProjectsAsync(GridState<ProjectDto.Index> state) {
		// Use centralized fetch to avoid duplicating error handling
		int skip = state.Page * state.PageSize;
		int take = state.PageSize;
		// OrderBy can be adapted to use state sorting if available; keep "Id" to match previous behavior
		Result<ProjectResponse.Index>? result = await FetchProjectsAsync(skip, take, "Id");

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

	private static QueryRequest.SkipTake BuildRequest(int skip, int take, string? orderBy) {
		return new QueryRequest.SkipTake {
			Skip = skip,
			Take = take,
			OrderBy = orderBy
		};
	}

	// --- Centralized fetch used by both grid and mobile -----------------

	/// <summary>
	///     Centralized fetch that builds the request, calls the service and handles logging/errors.
	///     Returns null on exception so callers can handle fallback behaviour.
	/// </summary>
	private async Task<Result<ProjectResponse.Index>?> FetchProjectsAsync(int skip, int take, string? orderBy = null) {
		try {
			QueryRequest.SkipTake req = BuildRequest(skip, take, orderBy);
			Result<ProjectResponse.Index> result = await ProjectService.GetIndexAsync(req);
			return result;
		} catch (Exception ex) {
			// Single place for logging errors for all callers
			Log.Error(ex, "FetchProjectsAsync failed (skip={Skip}, take={Take}, orderBy={OrderBy})", skip, take,
				orderBy);
			return null;
		}
	}

	// --- Mobile data (cards) --------------------------------------------

	// Default parameter values make fields unnecessary; call without parameters for page 0 / size 10
	private async Task FetchMobileItemsAsync(int page = 0, int pageSize = 10, string? orderBy = null) {
		Result<ProjectResponse.Index>? response = await FetchProjectsAsync(page * pageSize, pageSize, orderBy);

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
		if (id is null) {
			return;
		}

		NavigationManager.NavigateTo($"/projects/{id}");
	}

	public Task OpenSortOptionsAsync() {
		_mobileSortOptionsOpen = true;
		StateHasChanged();
		return Task.CompletedTask;
	}

	private async Task SortMobileAsync(MobileProjectSortColumn column, bool descending) {
		if (_mobileItems is null || _mobileItems.Count == 0) {
			_mobileSortOptionsOpen = false;
			StateHasChanged();
			return;
		}

		IEnumerable<ProjectDto.Index> ordered = column switch {
			MobileProjectSortColumn.Code => descending
				? _mobileItems.OrderByDescending(p => p.Code)
				: _mobileItems.OrderBy(p => p.Code),

			MobileProjectSortColumn.Name => descending
				? _mobileItems.OrderByDescending(p => p.Name)
				: _mobileItems.OrderBy(p => p.Name),

			MobileProjectSortColumn.StartDate => descending
				? _mobileItems.OrderByDescending(p => p.StartDate)
				: _mobileItems.OrderBy(p => p.StartDate),

			MobileProjectSortColumn.EndDate => descending
				? _mobileItems.OrderByDescending(p => p.EndDate)
				: _mobileItems.OrderBy(p => p.EndDate),

			MobileProjectSortColumn.MeasurementNetwork => descending
				? _mobileItems.OrderByDescending(p => p.MeasurementNetwork)
				: _mobileItems.OrderBy(p => p.MeasurementNetwork),

			MobileProjectSortColumn.Zone => descending
				? _mobileItems.OrderByDescending(p => p.Zone)
				: _mobileItems.OrderBy(p => p.Zone),

			MobileProjectSortColumn.Status => descending
				? _mobileItems.OrderByDescending(p => p.Status)
				: _mobileItems.OrderBy(p => p.Status),

			_ => descending
				? _mobileItems.OrderByDescending(p => p.Name)
				: _mobileItems.OrderBy(p => p.Name)
		};

		_mobileItems = ordered.ToList();

		_mobileSortOptionsOpen = false;

		await InvokeAsync(StateHasChanged);
	}
}