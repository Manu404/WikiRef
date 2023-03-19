using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace WikiRef
{
    class FileHelper
    {
        private ConsoleHelper _consoleHelper;

        public FileHelper(ConsoleHelper consoleHelper)
        {
            _consoleHelper = consoleHelper;
        }

        public void SaveConsoleOutputToFile()
        {
            string filename = GenerateFileName(".txt");
            try
            {
                using (TextWriter textWritter = new StreamWriter(filename))
                    foreach (String line in _consoleHelper.TextBuffer)
                        textWritter.WriteLine(line);
            }
            catch (Exception e)
            {
                _consoleHelper.WriteLineInRed(String.Format("An error occured while saving console output to file {0}", filename));
                _consoleHelper.WriteLineInRed(e.Message);
            }
        }

        public void SaveJsonToFile(List<WikiPage> pages)
        {
            string filename = GenerateFileName(".json");
            try
            {
                using (TextWriter textWritter = new StreamWriter(filename))
                {
                    string output = JsonConvert.SerializeObject(pages);
                    textWritter.WriteLine(output);
                }
                _consoleHelper.WriteLine(String.Format("Json saved to: {0}", filename));
            }
            catch (Exception e)
            {
                _consoleHelper.WriteLineInRed(String.Format("An error occured while saving console output to file {0}", filename));
                _consoleHelper.WriteLineInRed(e.Message);
            }
        }

        private static string GenerateFileName(string extension)
        {
            string filenameWithtoutExtension = String.Format("output_{0}", DateTime.Now.ToString("yyyyMMddTHH-mm-ss"));
            string filenameWithExtension = String.Format("{0}{1}", filenameWithtoutExtension, extension);

            // add '_' until find unique file name, barely possible, but possible.
            while (File.Exists(filenameWithExtension))
            {
                filenameWithtoutExtension += "_";
                filenameWithExtension = String.Format("{0}{1}", filenameWithtoutExtension, extension);
            }

            return filenameWithExtension;
        }
    }
}
