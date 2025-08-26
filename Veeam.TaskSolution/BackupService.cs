using Serilog;
using Veeam.TaskSolution.Utils;

namespace Veeam.TaskSolution;

public class BackupService
{
    static ILogger logger => LoggerConfig.GetLogger<BackupService>();
    CancellationTokenSource cts { get; set; }
    ServiceConfig config { get; set; }

    public BackupService(ServiceConfig config)
    {
        cts = new();
        this.config = config;
    }

    public void StartSync()
    {
        Task.Run(async () =>
        {
            config.BackupDir.EnsureDirectoryExists();

            while (true)
            {
                try
                {
                    CleanBackupDirectory();
                    ReplicateSource();
                    logger.Information("Synchronization completed at {time}", DateTimeOffset.Now);
                }
                catch (Exception ex)
                {
                    logger.Error(ex, "An error occurred during synchronization at {time}", DateTimeOffset.Now);
                }

                await Task.Delay(config.SyncInterval);
            }
        });
    }

    public void CleanBackupDirectory()
    {
        var dirsInSource = config.SourceDir.GetAllrelativedirs();
        var dirsInBackup = config.BackupDir.GetAllrelativedirs();
        var deletedDirs = dirsInBackup.Except(dirsInSource).ToList();

        logger.Debug("Directories to be deleted: {deletedDirs}", deletedDirs);

        foreach (var dir in deletedDirs)
        {
            var dirToDelete = Path.Combine(config.BackupDir, dir);
            if (Directory.Exists(dirToDelete))
            {
                Directory.Delete(dirToDelete, true);
                logger.Information("Deleted directory: {dir}", dirToDelete);
            }
        }

        var filesInSource = config.SourceDir.GetAllFiles().Select(file => file.GetRelativePath(config.SourceDir)).ToList();
        var filesInBackup = config.BackupDir.GetAllFiles().Select(file => file.GetRelativePath(config.BackupDir)).ToList();
        var deletedFiles = filesInBackup.Except(filesInSource).ToList();

        logger.Debug("Files to be deleted: {deletedFiles}", deletedFiles);

        foreach (var file in deletedFiles)
        {
            var fileToDelete = Path.Combine(config.BackupDir, file);
            fileToDelete.DeleteFile();
            logger.Information("Deleted file: {file}", file);
        }
    }

    public void ReplicateSource()
    {
        var filesInSource = config.SourceDir.GetAllFiles().Select(file => file.GetRelativePath(config.SourceDir)).ToList();
        var filesInBackup = config.BackupDir.GetAllFiles().Select(file => file.GetRelativePath(config.BackupDir)).ToList();

        foreach (var file in filesInSource)
        {
            var sourceFilePath = Path.Combine(config.SourceDir, file);
            var targetFilePath = Path.Combine(config.BackupDir, file);

            if (!filesInBackup.Contains(file) || !sourceFilePath.IsSameAs(targetFilePath))
            {
                targetFilePath.GetPath().EnsureDirectoryExists();
                sourceFilePath.CopyToDir(targetFilePath);
                logger.Information("Copied/Updated file: {file}", targetFilePath);
            }
        }

        var dirsInSource = config.SourceDir.GetAllrelativedirs();
        var dirsInBackup = config.BackupDir.GetAllrelativedirs();
        var missingDirs = dirsInSource.Except(dirsInBackup).ToList();

        logger.Debug("Directories to be created: {missingDirs}", missingDirs);

        foreach (var dir in missingDirs)
        {
            var targetPath = Path.Combine(config.BackupDir, dir);
            targetPath.EnsureDirectoryExists();
            logger.Information("Created directory: {targetPath}", targetPath);
        }
    }

    public void Stop()
    {
        cts.Cancel();
        logger.Information("Backup service stopped at {time}", DateTimeOffset.Now);
    }
}
