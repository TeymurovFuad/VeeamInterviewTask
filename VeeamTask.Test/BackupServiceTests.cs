using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Veeam.TaskSolution;
using Veeam.TaskSolution.Utils;

namespace VeeamTask
{
    [TestFixture]
    public class BackupServiceTests
    {
        BackupService fileManager;
        ServiceConfig config;
        string currentProjectDirRoot => Directory.GetParent(Directory.GetCurrentDirectory())!.Parent!.Parent!.FullName;
        string sourceDir => Path.Combine(currentProjectDirRoot, "Source");
        string backupDir => Path.Combine(currentProjectDirRoot, "Replica");
        string logFilePath => Path.Combine(currentProjectDirRoot, "Logs");

        [SetUp]
        public void Setup()
        {
            config = new ServiceConfig(
                    sourceDir: sourceDir,
                    backupDir: backupDir,
                    syncInterval: TimeSpan.FromSeconds(10),
                    logFilePath: logFilePath);

            LoggerConfig.InitLogger(config.LogFilePath);
            fileManager = new BackupService(config);
            fileManager.StartSync();

            sourceDir.EnsureDirectoryExists();
        }

        [Test]
        public async Task TestFileCopy()
        {
            var testFile1 = Path.Combine(sourceDir, "test1.txt");
            var testFile2 = Path.Combine(sourceDir, "test2.txt");

            await File.WriteAllTextAsync(testFile1, "This is a test file 1.");
            await File.WriteAllTextAsync(testFile2, "This is a test file 2.");

            // Wait for the sync interval plus a buffer
            await Task.Delay(config.SyncInterval*2);

            var backupFile1 = Path.Combine(backupDir, "test1.txt");
            var backupFile2 = Path.Combine(backupDir, "test2.txt");

            Assert.Multiple(() =>
            {
                Assert.That(File.Exists(backupFile1), Is.True, "Test file 1 was not copied to backup directory.");
                Assert.That(File.Exists(backupFile2), Is.True, "Test file 2 was not copied to backup directory.");
            });

            var content1 = await File.ReadAllTextAsync(backupFile1);
            var content2 = await File.ReadAllTextAsync(backupFile2);

            Assert.Multiple(() =>
            {
                Assert.That(content1, Is.EqualTo("This is a test file 1."), "Content of test file 1 does not match.");
                Assert.That(content2, Is.EqualTo("This is a test file 2."), "Content of test file 2 does not match.");
            });
        }
    }
}
