using Veeam.TaskSolution;
using Veeam.TaskSolution.Utils;


void StartService(string[] args)
{
    if (args.Length != 4)
        throw new ArgumentException("Usage: VeeamTask.Runner <sourceDir> <backupDir> <syncIntervalInSeconds> <logFilePath>");

    if (!int.TryParse(args[2], out var syncIntervalInSeconds) || syncIntervalInSeconds <= 0)
        throw new ArgumentException("Sync interval must be a positive integer representing seconds.");

    var sourceDir = args[0];

    if(string.IsNullOrEmpty(sourceDir?.Trim()))
        throw new ArgumentException("Source directory cannot be null or empty.");
    else if (!Directory.Exists(sourceDir))
        throw new DirectoryNotFoundException($"Source directory '{sourceDir}' does not exist.");

    var backupDir = args[1];

    if (string.IsNullOrEmpty(backupDir?.Trim()))
        throw new ArgumentException("Backup directory cannot be null or empty.");

    var logFilePath = args[3];

    if (string.IsNullOrEmpty(logFilePath?.Trim()))
        throw new ArgumentException("Log file path cannot be null or empty.");


    Console.WriteLine("Starting Backup Service...");
    Console.WriteLine("Press Ctrl+C to stop the service.");

    var config = new ServiceConfig(
               sourceDir: sourceDir,
                backupDir: backupDir,
                syncInterval: TimeSpan.FromSeconds(syncIntervalInSeconds),
                logFilePath: logFilePath);

    LoggerConfig.InitLogger(config.LogFilePath);
    var fileManager = new BackupService(config);
    fileManager.StartSync();
}

StartService(args);