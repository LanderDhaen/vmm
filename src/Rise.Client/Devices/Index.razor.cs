using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Services;
using Rise.Shared.Common;
using Rise.Shared.Devices;
using Rise.Shared.Projects;

using DeviceDto = Rise.Shared.Devices.DeviceDto;

namespace Rise.Client.Devices;

public partial class Index {
	private static readonly (string Value, string Label)[] SortColumns = [
		("Serial", "Code"),
		("Name", "Naam"),
		("Type", "Type"),
		("Status", "Status")
	];

	private string _activeSorting = "Name";
	private bool _activeSortingDesc;

	private IEnumerable<DeviceDto.Index>? _mobileItems;

	private bool _mobileSortOptionsOpen;


	[Inject] public required IDeviceService DeviceService { get; init; }
	[Inject] public required NavigationManager NavigationManager { get; init; }
	[Inject] public required BreadcrumbService BreadcrumbService { get; init; }
	[Inject] public required HeaderService HeaderService { get; init; }
	[Inject] public required IBrowserViewportService ViewportService { get; init; }


	protected override async Task OnInitializedAsync() {
		BreadcrumbService.SetBreadcrumbs(
			new BreadcrumbItem("Onderhouds objecten", null, true)
		);

		HeaderService.Set("Onderhouds objecten");

		Breakpoint breakpoint = await ViewportService.GetCurrentBreakpointAsync();

		if (breakpoint == Breakpoint.Xs) {
			await GetDevicesMobileAsync(_activeSorting, _activeSortingDesc);
		}
	}

	// Desktop

	private async Task<GridData<DeviceDto.Index>> GetDevicesAsync(GridState<DeviceDto.Index> state) {
		SortDefinition<DeviceDto.Index>? sorting = state.SortDefinitions.FirstOrDefault();

		QueryRequest.SkipTake request = new() {
			Skip = state.Page * state.PageSize,
			Take = state.PageSize,
			OrderBy = sorting?.SortBy ?? "Name",
			OrderDescending = sorting?.Descending ?? false
		};

		Result<DeviceResponse.Index> result = await DeviceService.GetIndexAsync(request);

		return new GridData<DeviceDto.Index> {
			TotalItems = result.Value.TotalCount,
			Items = result.Value.Devices
		};
	}

	// Mobile

	private async Task GetDevicesMobileAsync(string orderBy, bool orderDescending) {
		QueryRequest.SkipTake request = new() {
			Skip = 0,
			Take = 10,
			OrderBy = orderBy,
			OrderDescending = orderDescending
		};

		Result<DeviceResponse.Index> result = await DeviceService.GetIndexAsync(request);

		_mobileItems = result.Value.Devices;
	}

	// Mobile Sorting

	private void ToggleSortingDrawer() {
		_mobileSortOptionsOpen = !_mobileSortOptionsOpen;
	}

	private async Task SortMobileAsync(string orderBy, bool descending) {
		await GetDevicesMobileAsync(orderBy, descending);
		ToggleSortingDrawer();
		_activeSorting = orderBy;
		_activeSortingDesc = descending;
	}

	// TODO: Implement filtering (updating the state for desktop or calling SortFilterAsync for mobile)

	// Navigation

	private void NavigateToDevice(Guid deviceId) {
		NavigationManager.NavigateTo($"/devices/{deviceId}");
	}

	private void NavigateToCreate() {
		NavigationManager.NavigateTo("/devices/create");
	}

	// Other

	private Color DefineColor(Status status) {
		return status == Status.Actief ? Color.Success : Color.Error;
	}
}