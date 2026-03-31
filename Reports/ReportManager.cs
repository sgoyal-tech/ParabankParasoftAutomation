using System;
using System.IO;

namespace Reports
{
    public class ReportTest
    {
        private readonly string _file;

        public string Name { get; }

        public ReportTest(string name)
        {
            Name = name;
            var dir = Path.Combine("TestResults", "reports");
            Directory.CreateDirectory(dir);
            _file = Path.Combine(dir, SanitizeFileName(name) + ".log");
            File.AppendAllText(_file, $"=== START TEST: {name} - {DateTime.UtcNow:O}\n");
        }

        public ReportTest Pass(string message)
        {
            File.AppendAllText(_file, $"PASS: {message}\n");
            return this;
        }

        public ReportTest Fail(string message)
        {
            File.AppendAllText(_file, $"FAIL: {message}\n");
            return this;
        }

        public ReportTest Info(string message)
        {
            File.AppendAllText(_file, $"INFO: {message}\n");
            return this;
        }

        public ReportTest AddScreenCaptureFromPath(string path)
        {
            File.AppendAllText(_file, $"SCREENSHOT: {path}\n");
            return this;
        }

        public ReportTest AssignCategory(string category)
        {
            File.AppendAllText(_file, $"CATEGORY: {category}\n");
            return this;
        }

        private static string SanitizeFileName(string name)
        {
            foreach (var c in Path.GetInvalidFileNameChars()) name = name.Replace(c, '_');
            return name;
        }
    }

    public static class ReportManager
    {
        public static ReportTest CreateTest(string name) => new ReportTest(name);
    }
}
