using BeachApi.Logging.Entities;
using Serilog.Core;
using Serilog.Events;

namespace BeachApi.Logging;

public sealed class SqlSink : ILogEventSink
{
    private readonly LoggingDataContext dataContext;

    public SqlSink(LoggingDataContext dataContext)
    {
        this.dataContext = dataContext;
    }


    public void Emit(LogEvent logEvent)
    {
        var log = new Log
        {
            Message = logEvent.RenderMessage(),
            Level = logEvent.Level,
            TimeStamp = logEvent.Timestamp.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"),
            Exception = logEvent.Exception?.ToString()
        };

        dataContext.Logs.Add(log);
        dataContext.SaveChanges();
    }
}