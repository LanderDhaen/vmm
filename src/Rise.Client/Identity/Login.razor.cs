using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

using Rise.Shared.Identity.Accounts;

namespace Rise.Client.Identity;

public partial class Login {
	private readonly AccountRequest.Login Model = new();
	private Result _result = new();

	[SupplyParameterFromQuery]
	private string? ReturnUrl { get; set; }

	[Inject]
	public required IAccountManager AccountManager { get; set; }

	[Inject]
	public required NavigationManager Navigation { get; set; }

	[Inject]
	public required AuthenticationStateProvider AuthProvider { get; set; }

	protected override async Task OnInitializedAsync() {
		var authState = await AuthProvider.GetAuthenticationStateAsync();
		var user = authState.User;

		if (user.Identity?.IsAuthenticated == true) {
			if (!string.IsNullOrEmpty(ReturnUrl)) {
				Navigation.NavigateTo(ReturnUrl, forceLoad: true);
				return;
			}

			Navigation.NavigateTo("/projects", forceLoad: true);
		}
	}

	public async Task LoginUser() {
		_result = await AccountManager.LoginAsync(Model.Email!, Model.Password!);

		if (_result.IsSuccess) {
			if (!string.IsNullOrWhiteSpace(ReturnUrl)) {
				Navigation.NavigateTo(ReturnUrl, forceLoad: true);
				return;
			}

			Navigation.NavigateTo("/projects", forceLoad: true);
		}
	}
}