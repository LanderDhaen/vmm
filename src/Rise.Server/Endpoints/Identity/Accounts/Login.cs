using Microsoft.AspNetCore.Identity;

using Rise.Shared.Identity.Accounts;

namespace Rise.Server.Endpoints.Identity.Accounts;

/// <summary>
///     Login Endpoint.
///     See https://fast-endpoints.com/
/// </summary>
/// <param name="signInManager"></param>
public class Login(SignInManager<IdentityUser> signInManager) : Endpoint<AccountRequest.Login, Result> {
	private const bool UseCookies = true;
	private const bool UseSessionCookies = true;

	public override void Configure() {
		Post("/api/identity/accounts/login");
		AllowAnonymous();
	}

	public override async Task<Result> ExecuteAsync(AccountRequest.Login req, CancellationToken ctx) {
		bool useCookieScheme = UseCookies || UseSessionCookies;
		bool isPersistent = UseCookies && !UseSessionCookies;
		signInManager.AuthenticationScheme =
			useCookieScheme ? IdentityConstants.ApplicationScheme : IdentityConstants.BearerScheme;

		SignInResult result = await signInManager.PasswordSignInAsync(req.Email!, req.Password!, isPersistent, true);

		if (result.RequiresTwoFactor) {
			if (!string.IsNullOrEmpty(req.TwoFactorCode)) {
				result = await signInManager.TwoFactorAuthenticatorSignInAsync(req.TwoFactorCode, isPersistent,
					isPersistent);
			} else if (!string.IsNullOrEmpty(req.TwoFactorRecoveryCode)) {
				result = await signInManager.TwoFactorRecoveryCodeSignInAsync(req.TwoFactorRecoveryCode);
			}
		}

		if (!result.Succeeded) {
			return Result.Unauthorized(result.ToString());
		}

		// The signInManager already produced the needed response in the form of a cookie or bearer token.

		return Result.Success();
	}
}