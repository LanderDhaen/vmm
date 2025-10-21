using Microsoft.AspNetCore.Components;

using Rise.Shared.Identity.Accounts;

namespace Rise.Client.Identity;

public partial class Login {
	private readonly AccountRequest.Login Model = new();
	private Result _result = new();
	[SupplyParameterFromQuery] private string? ReturnUrl { get; set; }
	[Inject] public required IAccountManager AccountManager { get; set; }
	[Inject] public required NavigationManager Navigation { get; set; }

	public async Task LoginUser() {
		_result = await AccountManager.LoginAsync(Model.Email!, Model.Password!);

		if (_result.IsSuccess && !string.IsNullOrEmpty(ReturnUrl)) {
			Navigation.NavigateTo(ReturnUrl);
		}
	}
}