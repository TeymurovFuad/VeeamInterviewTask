using Serilog;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Veeam.TaskSolution.Utils;

public class LoggerConfig
{
    private static ILogger log { get; set; }

    public static void InitLogger(string logFilePath)
    {
        log = new LoggerConfiguration()
            .WriteTo.File(Path.Combine(logFilePath, "log-"), rollingInterval: RollingInterval.Day)
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .MinimumLevel.Debug()
            .CreateLogger();
    }

    public static ILogger GetLogger<T>()
    {
        return log.ForContext<T>();
    }

    public static ILogger GetLogger(Type context)
    {
        return log.ForContext(context);
    }

    public static void CloseAndFlush()
    {
        Log.CloseAndFlush();
    }
}
