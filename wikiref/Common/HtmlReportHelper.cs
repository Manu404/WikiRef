using System;
using System.Text;

namespace WikiRef
{
    class HtmlReportHelper
    {
        StringBuilder stringBuilder = new StringBuilder();

        public HtmlReportHelper()
        {
            Initialize();
        }

        private void Initialize()
        {
            stringBuilder.AppendLine("<html>");
            stringBuilder.AppendLine("<body style=\"background-color: black; font-family: consolas\">");
            AddLine(String.Format(new string('-', 20)), ConsoleColor.White);
            AddLine(String.Format("File generated on {0}", DateTime.Now.ToString("dddd, dd MMMM yyyy HH:mm:ss")), ConsoleColor.Green);
            AddLine(String.Format(new string('-', 20)), ConsoleColor.White);
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

        public string BuildReportContent()
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
}
