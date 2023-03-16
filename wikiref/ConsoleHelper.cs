using System;
using System.Collections.Generic;
using System.Text;

namespace WikiRef
{
    class ConsoleHelper
    {
        GlobalConfiguration _config;

        public List<string> TextBuffer { get; set; }

        public ConsoleHelper(GlobalConfiguration config)
        {
            TextBuffer = new List<string>();

            _config = config;
        }

        public void WriteLine(string text)
        {
            if (_config.Silent) return;
            Console.WriteLine(text);
            TextBuffer.Add(text);
        }

        public void WriteLineInRed(string text)
        {
            if (_config.Silent) return;
            if (_config.NoColor) WriteLine(text);
            else WriteLineInColor(text, ConsoleColor.Red);
            TextBuffer.Add(text);
        }

        public void WriteLineInGreen(string text)
        {
            if (_config.Silent) return;
            if (_config.NoColor) WriteLine(text);
            else WriteLineInColor(text, ConsoleColor.Green);
            TextBuffer.Add(text);
        }

        public void WriteLineInOrange(string text)
        {
            if (_config.Silent) return;
            if (_config.NoColor) WriteLine(text);
            else WriteLineInColor(text, ConsoleColor.Yellow);
            TextBuffer.Add(text);
        }

        public void WriteLineInGray(string text)
        {
            if (_config.Silent) return;
            if (_config.NoColor) WriteLine(text);
            else WriteLineInColor(text, ConsoleColor.DarkGray);
            TextBuffer.Add(text);
        }

        public void DisplayCheckingPageMessage(string pageName)
        {
            if (_config.Silent) return;
            Console.WriteLine(String.Format(new string('-', 20)));
            Console.WriteLine(String.Format("Analyzing page: {0}...", pageName));
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
