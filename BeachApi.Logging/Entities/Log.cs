using Serilog.Events;

namespace BeachApi.Logging.Entities;

public class Log
{
    public int Id { get; set; }

    public string Message { get; set; }

    public LogEventLevel? Level { get; set; }

    public string TimeStamp { get; set; }

    public string Exception { get; set; }
}