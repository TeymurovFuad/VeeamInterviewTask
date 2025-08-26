using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Veeam.TaskSolution.Utils
{
    public record ServiceConfig
    {
        public string SourceDir { get; init; }
        public string BackupDir { get; init; }
        public TimeSpan SyncInterval { get; init; }
        public string LogFilePath { get; init; }

        public ServiceConfig(string sourceDir, string backupDir, TimeSpan syncInterval, string logFilePath)
        {
            SourceDir = sourceDir;
            BackupDir = backupDir;
            SyncInterval = syncInterval;
            LogFilePath = logFilePath;
        }
    }
}
