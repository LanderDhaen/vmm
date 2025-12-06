using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Services;
using Rise.Shared.Common;
using Rise.Shared.WorkOrders;

namespace Rise.Client.WorkOrders;

public partial class Detail {
	private readonly List<string> _tabs = ["Informatie", "Historiek"];
	private int _selectedTab;

	private WorkOrderDto.Detail? _workOrder;

	[Parameter] public Guid WorkOrderId { get; set; }

	[Inject] public required IWorkOrderService WorkOrderService { get; set; }
	[Inject] public required BreadcrumbService BreadcrumbService { get; set; }
	[Inject] public required HeaderService HeaderService { get; set; }
	[Inject] private ISnackbar Snackbar { get; set; } = default!;
	[Inject] private NavigationManager Navigation { get; set; } = default!;

	protected override async Task OnInitializedAsync() {
		await GetWorkOrderDetailAsync();

		if (_workOrder is not null) {
			BreadcrumbService.SetBreadcrumbs(
				new BreadcrumbItem("Werkbonnen", "/workorders"),
				new BreadcrumbItem(_workOrder.Name, null, true)
			);

			HeaderService.Set(
				"Detail",
				Icons.Material.Filled.ArrowBack,
				() => {
					Navigation.NavigateTo("/workorders/");
					return Task.CompletedTask;
				}
			);
		}
	}

	private void SetTab(int index) => _selectedTab = index;

	private async Task GetWorkOrderDetailAsync() {
		Result<WorkOrderResponse.Detail> detail =
			await WorkOrderService.GetDetailAsync(WorkOrderId);

		if (detail.IsSuccess && detail.Value is not null) {
			_workOrder = detail.Value.WorkOrder;
		} else {
			string message = detail.Errors != null
				? string.Join(Environment.NewLine, detail.Errors)
				: "Er is een fout opgetreden.";
			Snackbar.Add(message, Severity.Error);
		}
	}

	private void NavigateToMaintObj(Guid id) {
		Navigation.NavigateTo($"/maintobjects/{id}");
	}
}