using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Reports
{
    public class ReportTest
    {
        public string Name { get; }
        public string Category { get; private set; } = string.Empty;
        public List<string> Messages { get; } = new List<string>();
        public string? ScreenshotPath { get; private set; }

        public ReportTest(string name, string category)
        {
            Name = name;
            Category = category ?? string.Empty;
            Messages.Add($"Started: {DateTime.UtcNow:O}");
        }

        public ReportTest Pass(string message) { Messages.Add("PASS: " + message); return this; }
        public ReportTest Fail(string message) { Messages.Add("FAIL: " + message); return this; }
        public ReportTest Info(string message) { Messages.Add("INFO: " + message); return this; }
        public ReportTest AddScreenCaptureFromPath(string path) { ScreenshotPath = path; Messages.Add("SCREENSHOT: " + path); return this; }
        public ReportTest AssignCategory(string category) { Category = category; return this; }
    }

    public static class ReportManager
    {
        private static readonly List<ReportTest> _tests = new List<ReportTest>();
        private static string _resultsDir = Path.Combine(Directory.GetCurrentDirectory(), "TestResults");

        public static ReportTest CreateTest(string name, string resultsDir)
        {
            if (!string.IsNullOrEmpty(resultsDir)) _resultsDir = resultsDir;
            var t = new ReportTest(name, string.Empty);
            _tests.Add(t);
            // also ensure per-test log file exists
            var dir = Path.Combine(_resultsDir, "reports");
            Directory.CreateDirectory(dir);
            var file = Path.Combine(dir, SanitizeFileName(name) + ".log");
            File.AppendAllText(file, $"=== START TEST: {name} - {DateTime.UtcNow:O}\n");
            return t;
        }

        public static void Flush()
        {
            try
            {
                var dir = Path.Combine(_resultsDir, "reports");
                Directory.CreateDirectory(dir);
                var html = new StringBuilder();
                html.AppendLine("<html><head><meta charset=\"utf-8\"><title>Test Report</title>");
                html.AppendLine("<style>body{font-family:Segoe UI,Arial} .pass{color:green}.fail{color:red}</style>");
                html.AppendLine("</head><body>");
                html.AppendLine($"<h1>Test Report - {DateTime.UtcNow:O}</h1>");
                html.AppendLine("<ul>");
                foreach (var t in _tests)
                {
                    var status = t.Messages.Exists(m => m.StartsWith("FAIL:")) ? "fail" : "pass";
                    html.AppendLine($"<li class='{status}'><strong>{t.Name}</strong> [{t.Category}]<br/>");
                    html.AppendLine("<ul>");
                    foreach (var m in t.Messages)
                        html.AppendLine($"<li>{System.Net.WebUtility.HtmlEncode(m)}</li>");
                    if (!string.IsNullOrEmpty(t.ScreenshotPath)) html.AppendLine($"<li>Screenshot: <a href=\"{t.ScreenshotPath}\">{t.ScreenshotPath}</a></li>");
                    html.AppendLine("</ul></li>");
                }
                html.AppendLine("</ul></body></html>");
                var outFile = Path.Combine(dir, "report.html");
                File.WriteAllText(outFile, html.ToString());
            }
            catch { /* ignore */ }
        }

        private static string SanitizeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
            return name;
        }
    }
}
