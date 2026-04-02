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
            // Ensure results directory is available (can be overridden by CI via TEST_RESULTS_DIR)
            ResultsDir = Environment.GetEnvironmentVariable("TEST_RESULTS_DIR") ?? Path.Combine(Directory.GetCurrentDirectory(), "TestResults");
            try { Directory.CreateDirectory(ResultsDir); } catch { }

            // Ensure ChromeDriver binary is available and matches installed Chrome
            // Attempt to detect the installed Chrome/Chromium version and request a matching driver.
            try
            {
                var detectedVersion = DetectInstalledChromeVersion();
                if (!string.IsNullOrEmpty(detectedVersion))
                {
                    // Request a driver that matches the exact browser version when possible
                    new DriverManager().SetUpDriver(new ChromeConfig(), detectedVersion);
                }
                else
                {
                    // Fallback to default behavior (WebDriverManager will pick a driver)
                    new DriverManager().SetUpDriver(new ChromeConfig());
                }
            }
            catch
            {
                // If detection fails for any reason, fallback to default behavior to avoid blocking tests
                try { new DriverManager().SetUpDriver(new ChromeConfig()); } catch { }
            }
            
            var options = new ChromeOptions();

            // Headless mode control
            // Headless is explicitly disabled here by default. To enable headless set this flag to true.
            var headlessEnabled = false; // <-- headless mode set to false

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

            // Initialize reporting and create a test node
            TestReport = Reports.ReportManager.CreateTest(TestContext.CurrentContext.Test.Name, ResultsDir);
            TestReport.AssignCategory(TestContext.CurrentContext.Test.ClassName ?? "Tests");
        }

        private static string? DetectInstalledChromeVersion()
        {
            try
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    // Check common install locations and CHROME_PATH env var
                    var env = Environment.GetEnvironmentVariable("CHROME_PATH");
                    if (!string.IsNullOrEmpty(env) && File.Exists(env))
                    {
                        var info = FileVersionInfo.GetVersionInfo(env);
                        return info.ProductVersion; // e.g. "146.0.7680.178"
                    }

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
                    // Try common commands to get version
                    string[] commands = { "google-chrome", "google-chrome-stable", "chromium-browser", "chromium" };
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

                            // Output examples:
                            // Google Chrome 146.0.7680.178
                            // Chromium 146.0.7680.178
                            var parts = outp.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                            foreach (var part in parts)
                            {
                                if (part.Length > 0 && char.IsDigit(part[0]) && part.Contains('.'))
                                {
                                    // crude validation
                                    return part.Trim();
                                }
                            }
                        }
                        catch { /* ignore and try next */ }
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
                // attach screenshot
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

            // Flush reports to ensure HTML report and logs are written into the results directory
            try { Reports.ReportManager.Flush(); } catch { }

            try { Driver.Quit(); } catch { }
            try { Driver.Dispose(); } catch { }
        }

        private static string SanitizeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
            return name;
        }
    }
}
