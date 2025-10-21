using Microsoft.AspNetCore.Components;

using Rise.Shared.Identity.Accounts;

namespace Rise.Client.Identity;

public partial class Register {
	private Result? _result;
	[Inject] public required IAccountManager AccountManager { get; set; }
	private AccountRequest.Register Model { get; } = new();

	public async Task RegisterUserAsync() {
		_result = await AccountManager.RegisterAsync(Model.Email!, Model.Password!, Model.ConfirmPassword!);
	}
}