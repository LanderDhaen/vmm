using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Components.MaintObjects;
using Rise.Client.Services;
using Rise.Shared.MaintObjects;

namespace Rise.Client.MaintObjects;

public partial class Detail {
	private readonly List<string> _tabs = [
		"Informatie",
		"Historiek"
	];

	private MaintObjDto.Detail? _maintObj;

	private int _selectedTab;

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

	private async Task OpenEditMaintObjDialogAsync() {
		DialogParameters<EditFormDialog> parameters = new() {
			{ x => x.MaintObjId, MaintObjId },
			{ x => x.MaintObj, _maintObj }
		};
		DialogOptions options = new() { CloseOnEscapeKey = true, CloseButton = true, MaxWidth = MaxWidth.ExtraLarge };

		IDialogReference dialog =
			await DialogService.ShowAsync<EditFormDialog>("Station bewerken", parameters, options);
		DialogResult? result = await dialog.Result;

		if (result is { Canceled: false }) {
			await RefreshMaintObjDetails();
		}
	}

	//helper
	private async Task RefreshMaintObjDetails() {
		Result<MaintObjResponse.Detail> result = await MaintObjService.GetDetailAsync(MaintObjId);

		if (result.IsSuccess) {
			_maintObj = result.Value.MaintObj;
		} else {
			string message = result.Errors != null
				? string.Join(Environment.NewLine, result.Errors)
				: "Er is een fout opgetreden.";
			Snackbar.Add(message, Severity.Error);
		}
	}
}