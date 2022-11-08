using BeachApi.Security;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace BeachApi.Logging;

public static class LoggerConfigurationExtensions
{
    public static LoggerConfiguration SqlClient(this LoggerSinkConfiguration sinkConfiguration,
        IConfiguration configuration, LogEventLevel restrictedToMinimumLevel = LevelAlias.Minimum,
        LoggingLevelSwitch levelSwitch = null)
    {
        var hashedConnectionString = configuration.GetConnectionString("SqlConnection");
        var connectionString = StringHasher.GetString(hashedConnectionString);

        var dataContext = new LoggingDataContext(connectionString);
        var sink = new SqlSink(dataContext);

        return sinkConfiguration.Sink(sink, restrictedToMinimumLevel, levelSwitch);
    }
}