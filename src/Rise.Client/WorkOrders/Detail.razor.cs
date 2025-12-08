using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Services;
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
	[Inject] private IDialogService DialogService { get; set; } = default!;

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

		if (detail is { IsSuccess: true, Value: not null }) {
			_workOrder = detail.Value.WorkOrder;
		} else {
			string message = detail.Errors != null && detail.Errors.Any()
				? string.Join(Environment.NewLine, detail.Errors)
				: "Er is een fout opgetreden.";
			Snackbar.Add(message, Severity.Error);
		}
	}

	private void NavigateToMaintObj(Guid id) {
		Navigation.NavigateTo($"/maintobjects/{id}");
	}

	private async Task Close() {
		Result result = await WorkOrderService.UpdateStatusAsync(new WorkOrderRequest.UpdateStatus {
			WorkOrderId = WorkOrderId,
			NewStatus = WorkOrderStatus.Completed
		});

		if (result.IsSuccess) {
			await GetWorkOrderDetailAsync();
			Snackbar.Add("Werkbon succesvol afgesloten!", Severity.Success);
			StateHasChanged();
		} else {
			string message = result.Errors != null
				? string.Join(Environment.NewLine, result.Errors)
				: "Er is een fout opgetreden.";
			Snackbar.Add(message, Severity.Error);
		}
	}

	private async Task Start() {
		Result result = await WorkOrderService.UpdateStatusAsync(new WorkOrderRequest.UpdateStatus {
			WorkOrderId = WorkOrderId,
			NewStatus = WorkOrderStatus.InProgress
		});

		if (result.IsSuccess) {
			await GetWorkOrderDetailAsync();

			if (_workOrder?.Status == WorkOrderStatus.Completed) {
				Snackbar.Add("Werkbon succesvol heropend!", Severity.Success);
			} else {
				Snackbar.Add("Werkbon succesvol gestart!", Severity.Success);
			}

			StateHasChanged();
		} else {
			string message = result.Errors != null
				? string.Join(Environment.NewLine, result.Errors)
				: "Er is een fout opgetreden.";
			Snackbar.Add(message, Severity.Error);
		}
	}

	private async Task Cancel() {
		Result result = await WorkOrderService.UpdateStatusAsync(new WorkOrderRequest.UpdateStatus {
			WorkOrderId = WorkOrderId,
			NewStatus = WorkOrderStatus.Cancelled
		});

		if (result.IsSuccess) {
			await GetWorkOrderDetailAsync();
			Snackbar.Add("Werkbon succesvol geannuleerd!", Severity.Success);
			StateHasChanged();
		} else {
			string message = result.Errors != null
				? string.Join(Environment.NewLine, result.Errors)
				: "Er is een fout opgetreden.";
			Snackbar.Add(message, Severity.Error);
		}
	}

	private async Task ShowCloseConfirmationAsync() {
		bool? result = await DialogService.ShowMessageBox(
			"Bevestiging",
			"Weet u zeker dat u deze werkbon wilt afsluiten?",
			"Bevestigen",
			"Annuleren",
			options: new DialogOptions { CloseOnEscapeKey = true }
		);

		if (result == true) {
			await Close();
		}
	}

	private async Task ShowStartConfirmationAsync() {
		string msg = _workOrder?.Status == WorkOrderStatus.Completed
			? "Weet u zeker dat u deze werkbon wilt heropenen?"
			: "Weet u zeker dat u deze werkbon wilt starten?";

		bool? result = await DialogService.ShowMessageBox(
			"Bevestiging",
			msg,
			"Bevestigen",
			"Annuleren",
			options: new DialogOptions { CloseOnEscapeKey = true }
		);

		if (result == true) {
			await Start();
		}
	}

	private async Task ShowCancelConfirmationAsync() {
		bool? result = await DialogService.ShowMessageBox(
			"Bevestiging",
			"Weet u zeker dat u deze werkbon wilt annuleren?",
			"Bevestigen",
			"Annuleren",
			options: new DialogOptions { CloseOnEscapeKey = true }
		);

		if (result == true) {
			await Cancel();
		}
	}
}