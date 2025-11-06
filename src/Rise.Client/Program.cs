using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using MudBlazor;
using MudBlazor.Services;

using Rise.Client;
using Rise.Client.Identity;
using Rise.Client.Locations;
using Rise.Client.Networks;
using Rise.Client.Products;
using Rise.Client.Projects;
using Rise.Shared.Locations;
using Rise.Client.Stations;
using Rise.Shared.Networks;
using Rise.Shared.Products;
using Rise.Shared.Projects;

try {
	WebAssemblyHostBuilder builder = WebAssemblyHostBuilder.CreateDefault(args);

	builder.RootComponents.Add<App>("#app");
	builder.RootComponents.Add<HeadOutlet>("head::after");


	Log.Logger = new LoggerConfiguration()
		.MinimumLevel.Information()
		.WriteTo.BrowserConsole(outputTemplate: "[{Timestamp:HH:mm:ss}{Level:u3}]{Message:lj} {NewLine}{Exception}")
		.CreateLogger();

	Log.Information("Starting web application");

	// register the cookie handler
	builder.Services.AddTransient<CookieHandler>();

	// set up authorization
	builder.Services.AddAuthorizationCore();

	// setup mud blazor
	builder.Services.AddMudServices(config => {
		config.SnackbarConfiguration.PositionClass = Defaults.Classes.Position.TopCenter;
		config.SnackbarConfiguration.SnackbarVariant = Variant.Filled;
	});

	// register the custom state provider
	builder.Services.AddScoped<AuthenticationStateProvider, CookieAuthenticationStateProvider>();
	// register the account management interface
	builder.Services.AddScoped(sp => (IAccountManager)sp.GetRequiredService<AuthenticationStateProvider>());

	// configure client for auth interactions
	builder.Services.AddHttpClient("SecureApi",
			opt => opt.BaseAddress = new Uri(builder.Configuration["BackendUrl"] ?? "https://localhost:5001"))
		.AddHttpMessageHandler<CookieHandler>();

	builder.Services.AddHttpClient<IProductService, ProductService>(client => {
		client.BaseAddress = new Uri(builder.Configuration["BackendUrl"] ?? "https://localhost:5001");
	});

	builder.Services.AddHttpClient<IProjectService, ProjectService>(client => {
		client.BaseAddress = new Uri(builder.Configuration["BackendUrl"] ?? "https://localhost:5001");
	});
	
	builder.Services.AddHttpClient<ILocationService, LocationService>(client => {
		client.BaseAddress = new Uri(builder.Configuration["BackendUrl"] ?? "https://localhost:5001");
	});
	
	builder.Services.AddHttpClient<INetworkService, NetworkService>(client => {
		client.BaseAddress = new Uri(builder.Configuration["BackendUrl"] ?? "https://localhost:5001");
	});
	
	builder.Services.AddHttpClient<StationService>(client => {
		client.BaseAddress = new Uri(builder.Configuration["BackendUrl"] ?? "https://localhost:5001");
	});

	await builder.Build().RunAsync();
} catch (Exception ex) {
	Log.Fatal(ex, "An exception occurred while creating the WASM host");
	throw;
}