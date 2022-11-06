using Microsoft.Data.SqlClient;
using Serilog.Core;
using Serilog.Events;
using System.Data;

namespace BeachApi.Logging;

public sealed class SqlSink : ILogEventSink
{
	private readonly string connectionString;

	public SqlSink(string connectionString)
	{
		this.connectionString = connectionString;
	}

	public void Emit(LogEvent logEvent)
	{
		using var connection = new SqlConnection(connectionString);
		connection.Open();

		var commandText = "INSERT INTO Logs(Message, Level, TimeStamp, Exception) ";
		commandText += "VALUES(@Message,@Level,@TimeStamp,@Exception)";

		using var command = connection.CreateCommand();
		command.CommandText = commandText;

		AddParameter("Message", DbType.String, logEvent.RenderMessage());
		AddParameter("Level", DbType.String, logEvent.Level);
		AddParameter("Timestamp", DbType.String, logEvent.Timestamp.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss"));
		AddParameter("Exception", DbType.String, logEvent.Exception?.ToString());

		command.ExecuteNonQuery();
		connection.Close();

		void AddParameter(string parameterName, DbType type, object value)
		{
			var parameter = command.CreateParameter();
			parameter.ParameterName = parameterName;
			parameter.DbType = type;
			parameter.Value = value ?? DBNull.Value;

			command.Parameters.Add(parameter);
		}
	}
}