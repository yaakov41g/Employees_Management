using EmployeeManagement.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EmployeeManagement.Controllers
{
    public class LogExampleController : Controller
    {
        private readonly ILogger<LogExampleController> Logger; // <LogExampleController>
        private readonly ILogger<LogExampleModel> mlogger1;

        public LogExampleController(ILogger<LogExampleController> logger, ILogger<LogExampleModel> mlogger) // <LogExampleController>
        {
            Logger = logger;
            mlogger1 = mlogger;
        }

        public void Index()
        {   Logger.LogTrace("Trace Log");
            Logger.LogDebug("Debug Log");
            Logger.LogInformation("Information Log");
            Logger.LogWarning("Warning Log");
            Logger.LogError("Error Log");
            Logger.LogCritical("Critical Log");
        }
        public void Repo()
        {
            LogExampleModel logExampleModel = new LogExampleModel(mlogger1);
            logExampleModel.LogLevels();
             }
    }
}
