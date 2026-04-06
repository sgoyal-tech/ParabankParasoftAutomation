using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using AventStack.ExtentReports.Reporter.Config;
using NUnit.Framework;
using OpenQA.Selenium;

namespace Reports;
    public class ReportTest
    {
        private readonly ExtentTest _test;

        public string Name { get; }
        public string Category { get; private set; } = string.Empty;
        public string? ScreenshotPath { get; private set; }
        public string Status { get; private set; } = "UNKNOWN";

        internal ReportTest(ExtentTest test, string name)
        {
            _test = test;
            Name = name;
            _test.Info($"Started: {DateTime.UtcNow:O}");
        }

        public ReportTest Pass(string message)
        {
            Status = "PASS";
            _test.Pass(message);
            return this;
        }

        public ReportTest Fail(string message)
        {
            Status = "FAIL";
            _test.Fail(message);
            return this;
        }

        public ReportTest Info(string message)
        {
            _test.Info(message);
            return this;
        }

        public ReportTest AddScreenCaptureFromPath(string path)
        {
            ScreenshotPath = path;
            _test.AddScreenCaptureFromPath(path);
            return this;
        }

        public ReportTest AssignCategory(string category)
        {
            Category = category;
            _test.AssignCategory(category);
            return this;
        }
    }

    public static class ReportManager
    {
        public static bool IsScreenShotsRequired = true;
        public static bool FailOnAssert = true;
        public static DateTime? TestStartDateTime;

        public static ExtentReports? extent;
        private static ExtentSparkReporter? htmlReporter;
        private static ExtentTest? parentTest;
        private static ExtentTest? childTest;
        private static string projectPath = Path.Combine(Directory.GetCurrentDirectory(), "TestResults");
        private static IWebDriver? currentDriver;
        private static readonly object SyncRoot = new();

        public static int stepCounter = 1;

        public static void RegisterDriver(IWebDriver driver)
        {
            currentDriver = driver;
        }

        public static void UnregisterDriver()
        {
            currentDriver = null;
        }

        public static void MoveResults()
        {
            lock (SyncRoot)
            {
                extent?.Flush();
            }
        }

        public static void FileSetup(string testContext)
        {
            lock (SyncRoot)
            {
                var resolvedResultsDir = ResolveResultsDirectory(testContext);
                if (!string.Equals(projectPath, resolvedResultsDir, StringComparison.OrdinalIgnoreCase))
                {
                    projectPath = resolvedResultsDir;
                    extent = null;
                    htmlReporter = null;
                    parentTest = null;
                    childTest = null;
                }

                if (extent != null)
                {
                    return;
                }

                var reportsDir = Path.Combine(projectPath, "reports");
                Directory.CreateDirectory(reportsDir);

                var reportPath = Path.Combine(reportsDir, "ExtentReport.html");
                htmlReporter = new ExtentSparkReporter(reportPath);
                htmlReporter.Config.DocumentTitle = "Automation Report";
                htmlReporter.Config.ReportName = "ParaBank Automation Test Report";
                htmlReporter.Config.Theme = Theme.Standard;
                htmlReporter.Config.Encoding = "utf-8";

                extent = new ExtentReports();
                extent.AttachReporter(htmlReporter);
                extent.AddSystemInfo("Machine", Environment.MachineName);
                extent.AddSystemInfo("OS", Environment.OSVersion.ToString());
                extent.AddSystemInfo(".NET", Environment.Version.ToString());
            }
        }

        public static void StartTestStep(string description)
        {
            if (parentTest == null)
                return;

            childTest = parentTest.CreateNode($"Test Step {stepCounter++}: {description}");
        }

        public static void StartMethod(string methodName)
        {
            if (!methodName.Contains("Method Started", StringComparison.OrdinalIgnoreCase))
            {
                methodName += " Method Started";
            }

            GetActiveTest()?.Log(Status.Info, methodName);
        }

        public static void StartTest(string testName)
        {
            FileSetup(projectPath);
            TestStartDateTime = DateTime.Now;
            parentTest = extent!.CreateTest(testName);
            childTest = null;
            stepCounter = 1;
        }

        public static void VerificationStep(string result, string details, string imageName = "SS_")
        {
            var activeTest = GetActiveTest();
            if (activeTest == null)
            {
                return;
            }

            var normalized = result?.Trim().ToLowerInvariant() ?? "";
            var imageFile = imageName + DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".png";
            var relPath = GetRelativeScreenshotPath(imageFile);

            if (normalized == "pass")
            {
                if (IsScreenShotsRequired)
                {
                    CaptureApplication(imageFile);
                    activeTest.Pass(details, MediaEntityBuilder.CreateScreenCaptureFromPath(relPath).Build());
                }
                else
                    activeTest.Pass(details);
                return;
            }

            if (normalized == "fail")
            {
                if (IsScreenShotsRequired)
                {
                    CaptureApplication(imageFile);
                    activeTest.Fail(details, MediaEntityBuilder.CreateScreenCaptureFromPath(relPath).Build());
                }
                else
                    activeTest.Fail(details);

                if (!details.Contains("Test Failed due to an unhandled exception", StringComparison.OrdinalIgnoreCase) && FailOnAssert)
                    Assert.Fail(details);
                return;
            }

            if (IsScreenShotsRequired)
            {
                CaptureApplication(imageFile);
                activeTest.Info(details, MediaEntityBuilder.CreateScreenCaptureFromPath(relPath).Build());
            }
            else
                activeTest.Info(details);
        }

        public static void TestStep(string stepDescription)
        {
            GetActiveTest()?.Log(Status.Info, stepDescription);
        }

        public static ReportTest CreateTest(string name, string resultsDir)
        {
            lock (SyncRoot)
            {
                FileSetup(resultsDir);
                StartTest(name);
                return new ReportTest(parentTest!, name);
            }
        }

        public static void Flush()
        {
            lock (SyncRoot)
            {
                try
                {
                    extent?.Flush();
                }
                catch
                {
                }
            }
        }

        private static void CaptureScreenshot()
        {
            var screenshotsDir = GetScreenshotsDirectory();
            Directory.CreateDirectory(screenshotsDir);
            var screenshotPath = Path.Combine(screenshotsDir, "Screenshot.png");

            var placeholderPng = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAusB9Wn0a9sAAAAASUVORK5CYII=");
            File.WriteAllBytes(screenshotPath, placeholderPng);
        }

        private static void CaptureApplication(string imageName)
        {
            try
            {
                var screenshotsDir = GetScreenshotsDirectory();
                Directory.CreateDirectory(screenshotsDir);
                var screenshotPath = Path.Combine(screenshotsDir, imageName);

                if (currentDriver is ITakesScreenshot screenshotDriver)
                {
                    var screenshot = screenshotDriver.GetScreenshot();
                    screenshot.SaveAsFile(screenshotPath);
                    return;
                }

                CaptureScreenshot();
                var defaultScreenshot = Path.Combine(screenshotsDir, "Screenshot.png");
                if (File.Exists(defaultScreenshot))
                {
                    File.Copy(defaultScreenshot, screenshotPath, true);
                }
            }
            catch
            {
            }
        }

                        private static ExtentTest? GetActiveTest()
                        {
                            return childTest ?? parentTest;
                        }

                        private static string ResolveResultsDirectory(string path)
                        {
                            if (string.IsNullOrWhiteSpace(path))
                            {
                                return projectPath;
                            }

                            if (Uri.TryCreate(path, UriKind.Absolute, out var uri) && uri.IsFile)
                            {
                                return Path.GetFullPath(uri.LocalPath);
                            }

                            return Path.GetFullPath(path);
                        }

                        private static string GetScreenshotsDirectory()
                        {
                            return Path.Combine(projectPath, "screenshots");
                        }

                        private static string GetRelativeScreenshotPath(string imageFileName)
                        {
                            return Path.Combine("..", "screenshots", imageFileName);
                        }
                    }
