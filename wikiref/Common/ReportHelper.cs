using System;
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
}
