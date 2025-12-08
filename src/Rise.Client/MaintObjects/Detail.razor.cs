using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Services;
using Rise.Shared.MaintObjects;
using Rise.Shared.Stations;

namespace Rise.Client.MaintObjects;

public partial class Detail {
	// ====== Tabs ======
	private readonly List<string> _tabs = [
		"Info",
		"Historiek"
	];

	private MaintObjDto.Detail? _maintObj;

	// Drawer
	// private bool _open;

	private int _selectedTab;
	private MeasurementStationDto.Index? _stationFromMaintObj;

	[Parameter] public Guid MaintObjId { get; set; }
	[Inject] public required IMaintObjService MaintObjService { get; set; }
	[Inject] public required BreadcrumbService BreadcrumbService { get; set; }
	[Inject] public required HeaderService HeaderService { get; set; }
	[Inject] private ISnackbar Snackbar { get; set; } = null!;
	[Inject] private IDialogService DialogService { get; set; } = null!;
	private bool IsActive => _maintObj?.MaintObjStatus == MaintObjStatus.Active;

	private void SetTab(int index) {
		_selectedTab = index;
	}

	protected override async Task OnInitializedAsync() {
		await GetMaintObjDetailAsync();
		// await GetStationAsync();

		if (_maintObj is not null) {
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
		}
	}

	private async Task GetMaintObjDetailAsync() {
		Result<MaintObjResponse.Detail> detail = await MaintObjService.GetDetailAsync(MaintObjId);

		// --- MaintObj detail ---
		if (detail.IsSuccess) {
			_maintObj = detail.Value.MaintObj;
		} else {
			string message = detail.Errors != null
				? string.Join(Environment.NewLine, detail.Errors)
				: "Er is een fout opgetreden.";
			Snackbar.Add(message, Severity.Error);
		}
	}
	//
	// private async Task GetStationAsync() {
	// 	Result<MaintObjResponse.MeasurementStation> stationFromProject =
	// 		await MaintObjService.GetStationAsync(MaintObjId, new QueryRequest.Search());
	//
	// 	// --- Station for this maintobj ---
	// 	if (stationFromProject is { IsSuccess: true, Value: not null }) {
	// 		_stationFromMaintObj = stationFromProject.Value.MeasurementStationFromMaintObj;
	// 	} else {
	// 		_stationFromMaintObj = null;
	//
	// 		string message = stationFromProject.Errors != null
	// 			? string.Join(Environment.NewLine, stationFromProject.Errors)
	// 			: "Kon meetstations niet laden.";
	// 		Snackbar.Add(message, Severity.Warning);
	// 	}
	// }
}