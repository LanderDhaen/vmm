using Microsoft.AspNetCore.Components;

using MudBlazor;

using Rise.Client.Services;
using Rise.Shared.Common;
using Rise.Shared.MaintObjects;
using Rise.Shared.Networks;
using Rise.Shared.Projects;

namespace Rise.Client.MaintObjects;

public partial class Create {
	private readonly MaintObjRequest.Create.Validator _validator = new();
	private Result<MaintObjResponse.Create>? _result;
	private MudForm form;
	[Inject] private ISnackbar Snackbar { get; set; }
	[Inject] public required IMaintObjService MaintObjService { get; set; }
	[Inject] public required NavigationManager NavigationManager { get; set; }
	[Inject] public required HeaderService HeaderService { get; set; }
	[Inject] public required BreadcrumbService BreadcrumbService { get; set; }

	private MaintObjRequest.Create Model { get; } = new();

	protected override async Task OnInitializedAsync() {
		BreadcrumbService.SetBreadcrumbs(
			new BreadcrumbItem("Onderhoudsobjecten", "/maintobjects"),
			new BreadcrumbItem("Voeg toe", "/create", true)
		);

		HeaderService.Set(
			"Voeg toe",
			Icons.Material.Filled.ArrowBack,
			() => {
				NavigationManager.NavigateTo("/maintobjects/");
				return Task.CompletedTask;
			}
		);
	}

	private async Task CreateMaintObjAsync() {
		await form.Validate();

		if (form.IsValid) {
			_result = await MaintObjService.CreateAsync(Model);
			if (_result.IsSuccess) {
				Snackbar.Add("Onderhoudsobject succesvol aangemaakt!", Severity.Success);
				await Task.Delay(2000);
				NavigationManager.NavigateTo("/maintobjects");
			} else {
				Snackbar.Add(string.Join("\n", _result.Errors), Severity.Error);
			}
		}
	}
}