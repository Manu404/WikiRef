using System;
using System.Collections.Generic;
using System.Text;

namespace WikiRef
{

    class ConsoleHelper
    {
        AppConfiguration _config;
        HtmlReportHelper _report;

        public StringBuilder Builder { get; private set; }

        public ConsoleHelper(AppConfiguration config, HtmlReportHelper reportHelper)
        {
            Builder = new StringBuilder();
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

        public void WriteInGrey(string text)
        {
            WriteIn(text, ConsoleColor.DarkGray, false);
        }

        public void WriteLineInDarkCyan(string text)
        {
            WriteIn(text, ConsoleColor.DarkCyan);
        }

        private void WriteIn(string text, ConsoleColor color, bool newLine = true)
        {
            Builder.Append(text);
            if (_config.Silent) return;
            if (_config.NoColor) WriteLineInColor(text, ConsoleColor.White);
            else WriteLineInColor(text, color, newLine);
        }

        public void WriteSection(string text)
        {
            WriteLine(string.Format(new string('-', 20)));
            WriteLine(text);
            if (_config.Silent) return;
            WriteLine(string.Format(new string('-', 20)));
            WriteLine(text);
        }

        private void WriteInColor(string text, ConsoleColor color)
        {
            var previousForeGround = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text);
            Builder.Append(text);
            _report.Add(text, color);
            Console.ForegroundColor = previousForeGround;
        }

        private void WriteLineInColor(string text, ConsoleColor color, bool newLine = true)
        {
            WriteInColor(text, color);
            if (newLine)
            {
                Console.Write(Environment.NewLine);
                _report.Add(Environment.NewLine, color);
                Builder.Append(Environment.NewLine);
            }
        }
    }
}
