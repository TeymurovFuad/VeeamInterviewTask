using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Veeam.TaskSolution.Utils
{
    public static class FileStorageExtensions
    {
        static ILogger logger => LoggerConfig.GetLogger(typeof(FileStorageExtensions));

        public static bool IsSameAs(this string targetFile, string sourceFile)
        {
            logger.Debug("Comparing files {targetFile} and {sourceFile}", targetFile, sourceFile);

            using var md5 = System.Security.Cryptography.MD5.Create();
            using var targetStream = File.OpenRead(targetFile);
            using var sourceStream = File.OpenRead(sourceFile);
            var hash1 = md5.ComputeHash(targetStream);
            var hash2 = md5.ComputeHash(sourceStream);

            var areEqual = hash1.SequenceEqual(hash2);

            logger.Debug("Hash of {targetFile}: {hash1}", targetFile, BitConverter.ToString(hash1).Replace("-", "").ToLowerInvariant());
            logger.Debug("Hash of {sourceFile}: {hash2}", sourceFile, BitConverter.ToString(hash2).Replace("-", "").ToLowerInvariant());
            logger.Information("Files {targetFile} and {sourceFile} are {areEqual}", targetFile, sourceFile, areEqual ? "the same" : "different");

            return areEqual;
        }

        public static bool IsFileUpdated(this string targetFile, string sourceFile)
        {
            var targetInfo = new FileInfo(targetFile);
            var sourceInfo = new FileInfo(sourceFile);
            var isUpdated = targetInfo.LastWriteTime < sourceInfo.LastWriteTime;

            logger.Information("File {targetFile} is {isUpdated} than {sourceFile}", targetFile, isUpdated ? "older" : "not older", sourceFile);

            return isUpdated;
        }

        public static List<string> GetAllFiles(this string directory)
        {
            var allFiles = Directory.GetFiles(directory, "*", SearchOption.AllDirectories).ToList();

            logger.Information("Retrieved {count} files from directory {directory}", allFiles.Count, directory);

            return allFiles;
        }

        public static List<string> GetAllDirectories(this string directory)
        {
            var allDirectories = Directory.GetDirectories(directory, "*", SearchOption.AllDirectories).ToList();

            logger.Information("Retrieved {count} directories from directory {directory}", allDirectories.Count, directory);

            return allDirectories;
        }

        public static List<string> GetAllrelativedirs(this string source)
        {
            var allDirectories = Directory.GetDirectories(source, "*", SearchOption.AllDirectories)
                                          .Select(d => Path.GetRelativePath(source, d))
                                          .ToList();

            logger.Information("Retrieved {count} relative directories from directory {source}", allDirectories.Count, source);

            return allDirectories;
        }

        public static void EnsureDirectoryExists(this string? directory)
        {
            if (string.IsNullOrEmpty(directory))
            {
                logger.Error("Directory path is null or empty.");
                ArgumentNullException.ThrowIfNull(directory);
            }

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                logger.Information("Created directory: {directory}", directory);
            }
        }

        public static string GetFileName(this string filePath)
        {
            return Path.GetFileName(filePath);
        }

        public static void CopyToDir(this string sourceFile, string targetDirectory)
        {
            File.Copy(sourceFile, targetDirectory, true);
            logger.Information("Copied file from {sourceFile} to {targetDirectory}", sourceFile, targetDirectory);
        }

        public static void DeleteFile(this string targetFile)
        {
            if (File.Exists(targetFile))
            {
                File.Delete(targetFile);
                logger.Information("Deleted file: {targetFile}", targetFile);
            }

            else
            {
                logger.Warning("Attempted to delete non-existent file: {targetFile}", targetFile);
            }
        }

        public static string GetPath(this string filePath)
        {
            return Path.GetDirectoryName(filePath)!;
        }

        public static string GetRelativePath(this string target, string basePath)
        {
            return Path.GetRelativePath(basePath, target);
        }
    }
}
