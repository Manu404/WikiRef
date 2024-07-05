using System;
using System.Text;

namespace WikiRef.Common
{
    public interface IConsoleHtmlBuffer
    {
        void Append(string text, ConsoleColor color);
        void AppendLine(string text, ConsoleColor color);
        string GetHtml();
    }

    public class ConsoleHtmlBuffer : IConsoleHtmlBuffer
    {
        StringBuilder _buffer = new StringBuilder();

        public ConsoleHtmlBuffer()
        {
            Initialize();
        }

        private void Initialize()
        {
            _buffer.AppendLine("<html>");
            _buffer.AppendLine("<body style=\"background-color: black; font-family: consolas\">");
            AppendLine(new string('-', 20), ConsoleColor.White);
            AppendLine($"File generated on {DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss")}", ConsoleColor.Green);
            AppendLine(new string('-', 20), ConsoleColor.White);
        }

        public string GetHtml()
        {
            EndBuffer();
            return _buffer.ToString();
        }

        public void Append(string text, ConsoleColor color)
        {
            _buffer.Append(String.Format("<span style=\"color: {0}\">{1}</span>", GetColorName(color), text));
        }

        public void AppendLine(string text, ConsoleColor color)
        {
            Append(text, color);
            Append(Environment.NewLine, color);
        }

        private void EndBuffer()
        {
            _buffer.Replace(Environment.NewLine, "</br>");
            _buffer.AppendLine("</body>");
            _buffer.AppendLine("</html>");
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
}
