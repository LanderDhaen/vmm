using Microsoft.Extensions.DependencyInjection;

using Rise.Persistence;
using Rise.Services.Locations;
using Rise.Services.MaintObjects;
using Rise.Services.Networks;
using Rise.Services.Projects;
using Rise.Services.Stations;
using Rise.Services.WorkOrders;
using Rise.Shared.Locations;
using Rise.Shared.MaintObjects;
using Rise.Shared.Networks;
using Rise.Shared.Projects;
using Rise.Shared.Stations;
using Rise.Shared.WorkOrders;

namespace Rise.Services;

public static class ServiceCollectionExtensions {
	public static IServiceCollection AddApplicationServices(this IServiceCollection services) {
		services.AddScoped<IProjectService, ProjectService>();
		services.AddScoped<ILocationService, LocationService>();
		services.AddScoped<INetworkService, NetworkService>();
		services.AddScoped<IMeasurementStationService, MeasurementStationService>();
		services.AddScoped<IMaintObjService, MaintObjService>();
		services.AddScoped<IWorkOrderService, WorkOrderService>();
		services.AddTransient<DbSeeder>();

		// Add other application services here.
		return services;
	}
}