using System;
using System.Collections.Generic;
using System.Text;

namespace WikiRef
{
    class ReportHelper
    {
        StringBuilder stringBuilder = new StringBuilder();

        public ReportHelper()
        {
            stringBuilder.AppendLine("<html>");
            stringBuilder.AppendLine("<body style=\"background-color: black; font-family: consolas\">");
        }

        public void AddLine(string text, ConsoleColor color)
        {
            stringBuilder.AppendLine(String.Format("<span style=\"color: {0}\">{1}</span></br>", GetColorName(color), text));
        }

        private void EndReport()
        {
            stringBuilder.AppendLine("</body>");
            stringBuilder.AppendLine("</html>");
        }

        public string GetReportContent()
        {
            EndReport();
            return stringBuilder.ToString();
        }

        private string GetColorName(ConsoleColor color)
        {
            switch (color)
            {
                case ConsoleColor.White:
                    return "white";
                case ConsoleColor.Red:
                    return "red";
                case ConsoleColor.Green:
                    return "lime";
                case ConsoleColor.Yellow:
                    return "yellow";
                case ConsoleColor.DarkGray:
                    return "gray";
                case ConsoleColor.DarkCyan:
                    return "teal";
                default:
                    return "white";
            }
        }
    }

    class ConsoleHelper
    {
        AppConfiguration _config;
        ReportHelper _report;

        public List<string> TextBuffer { get; set; }

        public ConsoleHelper(AppConfiguration config, ReportHelper reportHelper)
        {
            TextBuffer = new List<string>();
            _report = reportHelper;
            _config = config;
        }

        public void WriteLine(string text)
        {
            WriteIn(text, ConsoleColor.White);
        }

        public void WriteLineInRed(string text)
        {
            WriteIn(text, ConsoleColor.Red);
        }

        public void WriteLineInGreen(string text)
        {
            WriteIn(text, ConsoleColor.Green);
        }

        public void WriteLineInOrange(string text)
        {
            WriteIn(text, ConsoleColor.Yellow);
        }

        public void WriteLineInGray(string text)
        {
            WriteIn(text, ConsoleColor.DarkGray);
        }

        public void WriteLineInDarkCyan(string text)
        {
            WriteIn(text, ConsoleColor.DarkCyan);
        }

        private void WriteIn(string text, ConsoleColor color)
        {
            TextBuffer.Add(text);
            if (_config.Silent) return;
            if (_config.NoColor) WriteLine(text);
            else WriteLineInColor(text, color);
        }

        public void WriteSection(string text)
        {
            TextBuffer.Add(String.Format(new string('-', 20)));
            TextBuffer.Add(text);
            if (_config.Silent) return;
            WriteLine(String.Format(new string('-', 20)));
            WriteLine(text);
        }

        private void WriteLineInColor(string text, ConsoleColor color)
        {
            var previousForeGround = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(text);
            _report.AddLine(text, color);
            Console.ForegroundColor = previousForeGround;
        }
    }
}
