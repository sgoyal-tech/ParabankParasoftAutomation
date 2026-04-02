using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Reports
{
    public class ReportTest
    {
        private readonly string _file;
        public string Name { get; }
        public string Category { get; private set; } = string.Empty;
        public List<string> Messages { get; } = new List<string>();
        public string? ScreenshotPath { get; private set; }

        public ReportTest(string name, string resultsDir)
        {
            Name = name;
            var dir = Path.Combine(resultsDir, "reports");
            Directory.CreateDirectory(dir);
            _file = Path.Combine(dir, SanitizeFileName(name) + ".log");
            File.AppendAllText(_file, $"=== START TEST: {name} - {DateTime.UtcNow:O}\n");
            Messages.Add($"Started: {DateTime.UtcNow:O}");
        }

        private static string SanitizeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
            return name;
        }

        public ReportTest Pass(string message) { Messages.Add("PASS: " + message); File.AppendAllText(_file, $"PASS: {message}\n"); return this; }
        public ReportTest Fail(string message) { Messages.Add("FAIL: " + message); File.AppendAllText(_file, $"FAIL: {message}\n"); return this; }
        public ReportTest Info(string message) { Messages.Add("INFO: " + message); File.AppendAllText(_file, $"INFO: {message}\n"); return this; }
        public ReportTest AddScreenCaptureFromPath(string path) { ScreenshotPath = path; Messages.Add("SCREENSHOT: " + path); File.AppendAllText(_file, $"SCREENSHOT: {path}\n"); return this; }
        public ReportTest AssignCategory(string category) { Category = category; File.AppendAllText(_file, $"CATEGORY: {category}\n"); return this; }
    }

    public static class ReportManager
    {
        private static readonly List<ReportTest> _tests = new List<ReportTest>();
        private static string _resultsDir = Path.Combine(Directory.GetCurrentDirectory(), "TestResults");

        public static ReportTest CreateTest(string name, string resultsDir)
        {
            if (!string.IsNullOrEmpty(resultsDir)) _resultsDir = resultsDir;
            var t = new ReportTest(name, _resultsDir);
            _tests.Add(t);
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
