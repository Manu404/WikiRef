using System;
using System.Collections.Generic;
using System.Text;

namespace WikiRef
{
    class ConsoleHelper
    {
        public List<string> TextBuffer { get; set; }

        private bool IsSilent { get; set; }

        public ConsoleHelper(bool isSilent)
        {
            this.IsSilent = isSilent;
            TextBuffer = new List<string>();
        }
        public void WriteLine(string text)
        {
            if (IsSilent) return;
            Console.WriteLine(text);
            TextBuffer.Add(text);
        }

        public void WriteLineInRed(string text)
        {
            if (IsSilent) return;
            WriteLineInColor(text, ConsoleColor.Red);
            TextBuffer.Add(text);
        }

        public void WriteLineInGreen(string text)
        {
            if (IsSilent) return;
            WriteLineInColor(text, ConsoleColor.Green);
            TextBuffer.Add(text);
        }

        public void WriteLineInOrange(string text)
        {
            if (IsSilent) return;
            WriteLineInColor(text, ConsoleColor.Yellow);
            TextBuffer.Add(text);
        }

        public void WriteLineInGray(string text)
        {
            if (IsSilent) return;
            WriteLineInColor(text, ConsoleColor.DarkGray);
            TextBuffer.Add(text);
        }

        public void DisplayCheckingPageMessage(string pageName)
        {
            if (IsSilent) return;
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
