using Serilog;
using Serilog.Events;

namespace Charity.API.Services;

public static class ApplicationLogging
{
    public static LoggerConfiguration Configure(LoggerConfiguration configuration, string directory) => configuration
        .MinimumLevel.Information()
        .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
        .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Warning)
        .Enrich.FromLogContext()
        .Enrich.WithProperty("MachineName", Environment.MachineName)
        .WriteTo.Console()
        .WriteTo.File(
            path: Path.Combine(directory, "log-.log"),
            restrictedToMinimumLevel: LogEventLevel.Information,
            rollingInterval: RollingInterval.Day,
            retainedFileCountLimit: 30,
            fileSizeLimitBytes: null,
            shared: true,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {SourceContext} {Message:lj}{NewLine}Properties: {Properties:j}{NewLine}{Exception}{NewLine}");
}
