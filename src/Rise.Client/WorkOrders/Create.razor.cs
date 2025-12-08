using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Services;
using Rise.Shared.Common;
using Rise.Shared.MaintObjects;
using Rise.Shared.WorkOrders;

namespace Rise.Client.WorkOrders;

public partial class Create {
	[Inject] public IWorkOrderService WorkOrderService { get; set; } = default!;
	[Inject] public IMaintObjService MaintObjService { get; set; } = default!;
	[Inject] public ISnackbar Snackbar { get; set; } = default!;
	[Inject] public NavigationManager NavigationManager { get; set; } = default!;
	[Inject] public BreadcrumbService BreadcrumbService { get; set; } = default!;
	[Inject] public HeaderService HeaderService { get; set; } = default!;

	private readonly WorkOrderRequest.Create.Validator _validator = new();

	private MudForm form = default!;
	private WorkOrderRequest.Create Model { get; set; } = new();
	private IEnumerable<MaintObjDto.Index>? _maintObjs;

	protected override async Task OnInitializedAsync() {
		QueryRequest.SkipTake req = new() {
			Skip = 0,
			Take = 1000,
			OrderBy = "Name"
		};

		Result<MaintObjResponse.Index> moResult = await MaintObjService.GetIndexAsync(req);
		_maintObjs = moResult.Value.MaintObjects;

		BreadcrumbService.SetBreadcrumbs(
			new BreadcrumbItem("Werkbonnen", "/workorders"),
			new BreadcrumbItem("Voeg toe", "/workorders/create", true)
		);

		HeaderService.Set("Werkbon aanmaken", string.Empty);
	}

	private async Task CreateWorkOrderAsync() {
		await form.Validate();

		if (!form.IsValid)
			return;

		Result<WorkOrderResponse.Create> result =
			await WorkOrderService.CreateAsync(Model);

		if (result.IsSuccess) {
			Snackbar.Add("Werkbon succesvol aangemaakt!", Severity.Success);
			NavigationManager.NavigateTo("/workorders");
		} else {
			Snackbar.Add(string.Join("\n", result.Errors), Severity.Error);
		}
	}
}