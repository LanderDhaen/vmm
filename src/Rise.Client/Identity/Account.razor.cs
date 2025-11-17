using System.Security.Claims;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

using MudBlazor;

using Rise.Client.Services;

namespace Rise.Client.Identity;

public partial class Account {
	[Inject] public required IAccountManager AccountManager { get; set; }
	[Inject] public required AuthenticationStateProvider AuthProvider { get; set; }
	[Inject] public required NavigationManager Navigation { get; set; }

	[Inject] public required BreadcrumbService BreadcrumbService { get; set; }
	[Inject] public required HeaderService HeaderService { get; set; }

	private string? _email;
	private string _initials = "?";
	private List<string> _roles = [];
	private bool _isAuthenticated;
	private bool _isLoading = true;

	protected override async Task OnInitializedAsync() {
		await base.OnInitializedAsync();

		var authState = await AuthProvider.GetAuthenticationStateAsync();
		var user = authState.User;

		_isAuthenticated = user.Identity?.IsAuthenticated ?? false;

		if (_isAuthenticated) {
			_email = user.FindFirst(ClaimTypes.Email)?.Value
					 ?? user.Identity?.Name;

			_roles = user.FindAll(ClaimTypes.Role)
						 .Select(r => r.Value)
						 .ToList();

			_initials = GetInitialsFromEmail(_email);
		}

		BreadcrumbService.SetBreadcrumbs(
			new BreadcrumbItem("Home", "/"),
			new BreadcrumbItem("Account", null, true)
		);

		HeaderService.Set("Account");

		_isLoading = false;
	}

	private static string GetInitialsFromEmail(string? email) {
		if (string.IsNullOrWhiteSpace(email))
			return "?";

		var namePart = email.Split('@')[0];

		var parts = namePart
			.Split('.', '-', '_')
			.Where(p => !string.IsNullOrWhiteSpace(p))
			.Take(2)
			.ToArray();

		if (parts.Length == 0)
			return char.ToUpperInvariant(email[0]).ToString();

		var letters = parts
			.Select(p => char.ToUpperInvariant(p[0]))
			.ToArray();

		return new string(letters);
	}

	private async Task Logout() {
		await AccountManager.LogoutAsync();
		Navigation.NavigateTo("/login", forceLoad: true);
	}
}