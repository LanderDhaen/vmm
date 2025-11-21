using Microsoft.Extensions.DependencyInjection;

using Rise.Persistence;
using Rise.Services.Devices;
using Rise.Services.Locations;
using Rise.Services.Networks;
using Rise.Services.Projects;
using Rise.Services.Stations;
using Rise.Shared.Devices;
using Rise.Shared.Locations;
using Rise.Shared.Networks;
using Rise.Shared.Projects;
using Rise.Shared.Stations;

namespace Rise.Services;

public static class ServiceCollectionExtensions {
	public static IServiceCollection AddApplicationServices(this IServiceCollection services) {
		services.AddScoped<IProjectService, ProjectService>();
		services.AddScoped<ILocationService, LocationService>();
		services.AddScoped<INetworkService, NetworkService>();
		services.AddScoped<IMeasurementStationService, MeasurementStationService>();
		services.AddScoped<IDeviceService, DeviceService>();
		services.AddTransient<DbSeeder>();

		// Add other application services here.
		return services;
	}
}