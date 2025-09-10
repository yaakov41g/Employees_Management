using EmployeeManagement.Controllers;
using Microsoft.Extensions.Logging;

namespace EmployeeManagement.Models
{
    public class LogExampleModel // :ILogExampleModel
        {
        public ILogger<LogExampleModel> Logger;
        // I think there is already dependency registration for ILogger in some library
        // so we do not need for 'services.AddScoped' in 'Program.cs' 
        public LogExampleModel(ILogger<LogExampleModel> logger)
        {
            Logger = logger;
        }
        public void LogLevels()
        {
        //log methods for each log level   
        Logger.LogTrace("Trace Log");
        Logger.LogDebug("Debug Log");
        Logger.LogInformation("Information Log");
        Logger.LogWarning("Warning Log");
        Logger.LogError("Error Log");
        Logger.LogCritical("Critical Log");
        }
/*
       public LogExampleModel(ILogger<LogExampleController> logger1)
        {
            Logger = logger1;
        }

        public ILogger<LogExampleController> Logger { get; }*/
    }
}
