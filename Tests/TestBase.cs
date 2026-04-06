using AventStack.ExtentReports;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System;
using WebDriverManager;
using WebDriverManager.DriverConfigs.Impl;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace ParabankParasoftAutomation.Tests
{
    public abstract class TestBase
    {
        protected IWebDriver Driver = null!;
        protected string BaseUrl = "https://parabank.parasoft.com";
        protected Reports.ReportTest? TestReport;
        protected string ResultsDir = string.Empty;

        [SetUp]
        public void SetUp()
        {
            ResultsDir = Environment.GetEnvironmentVariable("TEST_RESULTS_DIR")
                ?? Path.Combine(Directory.GetCurrentDirectory(), "TestResults");
            try
            {
                Directory.CreateDirectory(ResultsDir);
            }
            catch
            {
            }

            // Match ChromeDriver to the installed Chrome version
            try
            {
                var detectedVersion = DetectInstalledChromeVersion();
                if (!string.IsNullOrEmpty(detectedVersion))
                    new DriverManager().SetUpDriver(new ChromeConfig(), detectedVersion);
                else
                    new DriverManager().SetUpDriver(new ChromeConfig());
            }
            catch
            {
                try
                {
                    new DriverManager().SetUpDriver(new ChromeConfig());
                }
                catch
                {
                }
            }
            
            var options = new ChromeOptions();
            var headlessEnabled = false;

            if (headlessEnabled)
            {
                options.AddArgument("--headless=new");
                options.AddArgument("--no-sandbox");
                options.AddArgument("--disable-dev-shm-usage");
                options.AddArgument("--window-size=1280,800");
            }

            Driver = new ChromeDriver(options);
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
            Driver.Manage().Window.Maximize();
            Reports.ReportManager.RegisterDriver(Driver);

            // Reporting
            TestReport = Reports.ReportManager.CreateTest(TestContext.CurrentContext.Test.Name, ResultsDir);
            TestReport.AssignCategory(TestContext.CurrentContext.Test.ClassName ?? "Tests");
            Reports.ReportManager.TestStep("Browser launched successfully.");
        }

        private static string? DetectInstalledChromeVersion()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    var env = Environment.GetEnvironmentVariable("CHROME_PATH");
                    if (!string.IsNullOrEmpty(env) && File.Exists(env))
                        return FileVersionInfo.GetVersionInfo(env).ProductVersion;

                    var progFiles = new[]
                    {
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles), "Google", "Chrome", "Application", "chrome.exe"),
                        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86), "Google", "Chrome", "Application", "chrome.exe")
                    };

                    foreach (var p in progFiles)
                    {
                        if (File.Exists(p))
                        {
                            var info = FileVersionInfo.GetVersionInfo(p);
                            return info.ProductVersion;
                        }
                    }

                    return null;
                }

                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) || RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    string[] commands = ["google-chrome", "google-chrome-stable", "chromium-browser", "chromium"];
                    foreach (var cmd in commands)
                    {
                        try
                        {
                            var psi = new ProcessStartInfo
                            {
                                FileName = cmd,
                                Arguments = "--version",
                                RedirectStandardOutput = true,
                                UseShellExecute = false,
                                CreateNoWindow = true
                            };

                            using var proc = Process.Start(psi);
                            if (proc == null) continue;
                            var outp = proc.StandardOutput.ReadToEnd();
                            proc.WaitForExit(2000);
                            if (string.IsNullOrEmpty(outp)) continue;

                            // Parse version number from output like "Google Chrome 146.0.7680.178"
                            var parts = outp.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            foreach (var part in parts)
                            {
                                if (part.Length > 0 && char.IsDigit(part[0]) && part.Contains('.'))
                                    return part.Trim();
                            }
                        }
                        catch { }
                    }

                    return null;
                }

                return null;
            }
            catch
            {
                return null;
            }
        }

        [TearDown]
        public void TearDown()
        {
            var outcome = TestContext.CurrentContext.Result.Outcome.Status;
            var msg = TestContext.CurrentContext.Result.Message;

            if (outcome == NUnit.Framework.Interfaces.TestStatus.Failed)
            {
                try
                {
                    var screenshotsDir = Path.Combine(ResultsDir, "screenshots");
                    Directory.CreateDirectory(screenshotsDir);
                    var file = Path.Combine(screenshotsDir, SanitizeFileName(TestContext.CurrentContext.Test.Name) + ".png");
                    var ss = ((ITakesScreenshot)Driver).GetScreenshot();
                    ss.SaveAsFile(file);
                    TestReport?.Fail(msg).AddScreenCaptureFromPath(file);
                }
                catch (Exception ex)
                {
                    TestReport?.Fail($"Failed to capture screenshot: {ex.Message}");
                }
            }
            else if (outcome == NUnit.Framework.Interfaces.TestStatus.Passed)
            {
                TestReport?.Pass("Test passed");
            }
            else
            {
                TestReport?.Info($"Test finished with status: {outcome}");
            }

            // Flush report and clean up
            try
            {
                Reports.ReportManager.Flush();
            }
            catch
            {
            }

            try
            {
                Reports.ReportManager.UnregisterDriver();
            }
            catch
            {
            }

            try
            {
                Driver.Quit();
            }
            catch
            {
            }

            try
            {
                Driver.Dispose();
            }
            catch
            {
            }
        }

        private static string SanitizeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }
            return name;
        }
    }
}
