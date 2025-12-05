using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Services;
using Rise.Shared.MaintObjects;

namespace Rise.Client.MaintObjects;

public partial class Detail {
	// ====== Tabs ======
	private readonly List<string> _tabs = new() {
		"Info",
		"Historiek"
	};

	// ====== Main ======
	private MaintObjDto.Detail? _maintObj;

	// Drawer
	// private bool _open;

	private int _selectedTab;

	[Parameter] public Guid MaintObjId { get; set; }
	[Inject] public required IMaintObjService MaintObjService { get; set; }
	[Inject] public required BreadcrumbService BreadcrumbService { get; set; }
	[Inject] public required HeaderService HeaderService { get; set; }
	[Inject] private ISnackbar Snackbar { get; set; } = null!;
	[Inject] private IDialogService DialogService { get; set; } = null!;
	private bool IsActive => _maintObj?.MaintObjStatus == MaintObjStatus.Actief;

	private void SetTab(int index) {
		_selectedTab = index;
	}

	protected override async Task OnInitializedAsync() {
		// Load project details and stations in parallel
		Task<Result<MaintObjResponse.Detail>> detailTask = MaintObjService.GetDetailAsync(MaintObjId);
		await Task.WhenAll(detailTask);

		Result<MaintObjResponse.Detail> detailResult = detailTask.Result;

		// --- Project detail ---
		if (detailResult.IsSuccess) {
			_maintObj = detailResult.Value.MaintObj;

			// set breadcrumbs
			BreadcrumbService.SetBreadcrumbs(
				new BreadcrumbItem("Onderhoudsobjecten", "/maintObjects"),
				new BreadcrumbItem($"{_maintObj.Name}", null, true)
			);

			// set header
			HeaderService.Set(
				"Detail",
				Icons.Material.Filled.ArrowBack,
				() => {
					Navigation.NavigateTo("/maintObjects/");
					return Task.CompletedTask;
				}
			);
		} else {
			string message = detailResult.Errors != null
				? string.Join(Environment.NewLine, detailResult.Errors)
				: "Er is een fout opgetreden.";
			Snackbar.Add(message, Severity.Error);
		}
	}
}