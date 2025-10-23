using Microsoft.Extensions.Logging;

using Serilog.Core;
using Serilog.Templates;

using Xunit.Abstractions;

namespace Xunit;

/// <summary>
///     Logging for xUnit helper class, taken from: https://bunit.dev/docs/misc-test-tips.html
/// </summary>
public static class ServiceCollectionLoggingExtensions {
	public static IServiceCollection AddXunitLogger(this IServiceCollection services, ITestOutputHelper outputHelper) {
		Logger serilogLogger = new LoggerConfiguration()
			.MinimumLevel.Verbose()
			.WriteTo.TestOutput(
				outputHelper,
				new ExpressionTemplate(
					"[{UtcDateTime(@t):mm:ss.ffffff} | {@l:u3} | {Substring(SourceContext, LastIndexOf(SourceContext, '.') + 1)} | {Coalesce(EventId.Name, '<none>')}] {@m}\n{@x}"))
			.CreateLogger();

		services.AddSingleton(_ => new LoggerFactory().AddSerilog(serilogLogger, true));
		services.AddSingleton(typeof(ILogger<>), typeof(Logger<>));
		return services;
	}
}