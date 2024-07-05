using System;
using System.Text;

namespace WikiRef.Common
{
    public interface IConsole
    {
        string GetTextBuffer();
        string GetHtml();
        void WriteInGrey(string text);
        void WriteLine(string text);
        void WriteLineInDarkCyan(string text);
        void WriteLineInGray(string text);
        void WriteLineInGreen(string text);
        void WriteLineInOrange(string text);
        void WriteLineInRed(string text);
        void WriteSection(string text);
    }

    public class ConsoleHelper : IConsole
    {
        IAppConfiguration _config;
        IConsoleHtmlBuffer _htmlBuffer;
        StringBuilder _textBuffer;

        public ConsoleHelper(IAppConfiguration config, IConsoleHtmlBuffer htmlBuffer)
        {
            _textBuffer = new StringBuilder();
            _htmlBuffer = htmlBuffer;
            _config = config;
        }

        public string GetTextBuffer()
        {
            return _textBuffer.ToString();
        }

        public string GetHtml()
        {
            return _htmlBuffer.GetHtml();
        }

        public void WriteInGrey(string text)
        {
            WriteInColor(text, ConsoleColor.DarkGray, false);
        }

        public void WriteLine(string text)
        {
            WriteInColor(text, ConsoleColor.White);
        }

        public void WriteLineInRed(string text)
        {
            WriteInColor(text, ConsoleColor.Red);
        }

        public void WriteLineInGreen(string text)
        {
            WriteInColor(text, ConsoleColor.Green);
        }

        public void WriteLineInOrange(string text)
        {
            WriteInColor(text, ConsoleColor.Yellow);
        }

        public void WriteLineInGray(string text)
        {
            WriteInColor(text, ConsoleColor.DarkGray);
        }

        public void WriteLineInDarkCyan(string text)
        {
            WriteInColor(text, ConsoleColor.DarkCyan);
        }

        public void WriteSection(string text)
        {
            WriteLine(string.Format(new string('-', 20)));
            WriteLine(text);
            WriteLine(string.Format(new string('-', 20)));
        }

        private void WriteInColor(string text, ConsoleColor color, bool newLine = true)
        {
            if(_config.NoColor) color = ConsoleColor.White;
            Write(text, color);
            if (newLine)
                Write(Environment.NewLine, color);
        }

        private void Write(string text, ConsoleColor color)
        {
            _textBuffer.Append(text);
            _htmlBuffer.Append(text, color);

            if (_config.Silent)
                return;

            var previousForeGround = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.Write(text);
            Console.ForegroundColor = previousForeGround;   
        }
    }
}
