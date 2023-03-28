using System;
using System.Collections.Generic;
using System.Text;

namespace WikiRef
{
    class ConsoleHelper
    {
        AppConfiguration _config;

        public List<string> TextBuffer { get; set; }

        public ConsoleHelper(AppConfiguration config)
        {
            TextBuffer = new List<string>();

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
            Console.ForegroundColor = previousForeGround;
        }
    }
}
