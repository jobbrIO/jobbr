using System;
using System.IO;
using System.Runtime.InteropServices;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Jobbr.Server.ForkedExecution.Tests
{
    [TestClass]
    public class ConfigurationValidatorTests
    {
        private readonly ConfigurationValidator _workingConfigurationValidator;
        private readonly ForkedExecutionConfiguration _workingConfiguration;

        public ConfigurationValidatorTests()
        {
            var loggerFactory = new LoggerFactory();
            _workingConfigurationValidator = new ConfigurationValidator(loggerFactory);
            _workingConfiguration = GetWorkingConfiguration();
        }

        [TestMethod]
        public void WorkingConfiguration_Validated_IsFine()
        {
            // Arrange
            // Act
            var validationResult = _workingConfigurationValidator.Validate(_workingConfiguration);

            // Assert
            Assert.IsTrue(validationResult);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RunnerExecutable_IsNull_ValidationThrowsException()
        {
            // Arrange
            // Act
            _workingConfiguration.JobRunnerExecutable = null;

            // Assert
            _workingConfigurationValidator.Validate(_workingConfiguration);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RunnerExecutable_Empty_ValidationThrowsException()
        {
            // Arrange
            // Act
            _workingConfiguration.JobRunnerExecutable = string.Empty;

            // Assert
            _workingConfigurationValidator.Validate(_workingConfiguration);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void RunnerExecutable_IsInvalidPath_ValidationThrowsException()
        {
            // Arrange
            // Act
            _workingConfiguration.JobRunnerExecutable = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "bla", "blupp.exe");

            // Assert
            _workingConfigurationValidator.Validate(_workingConfiguration);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void JobRunDirectory_IsNull_ValidationThrowsException()
        {
            // Arrange
            // Act
            _workingConfiguration.JobRunDirectory = null;

            // Assert
            _workingConfigurationValidator.Validate(_workingConfiguration);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void JobRunDirectory_Empty_ValidationThrowsException()
        {
            // Arrange
            // Act
            _workingConfiguration.JobRunDirectory = string.Empty;

            // Assert
            _workingConfigurationValidator.Validate(_workingConfiguration);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void JobRunDirectory_IsInvalidPath_ValidationThrowsException()
        {
            // Arrange
            // Act
            _workingConfiguration.JobRunDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "bla");

            // Assert
            _workingConfigurationValidator.Validate(_workingConfiguration);
        }

        private static ForkedExecutionConfiguration GetWorkingConfiguration()
        {
            return new ForkedExecutionConfiguration
            {
                JobRunnerExecutable = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.System), "cmd.exe") : "/bin/sh",
                BackendAddress = "http://localhost:1234",
                JobRunDirectory = Directory.GetCurrentDirectory(),
                MaxConcurrentProcesses = 4
            };
        }
    }
}
