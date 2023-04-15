using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;

namespace WikiRef
{
    class FileHelper
    {
        private ConsoleHelper _consoleHelper;
        private AppConfiguration _conf;

        public FileHelper(ConsoleHelper consoleHelper, AppConfiguration conf)
        {
            _consoleHelper = consoleHelper;
            _conf = conf;   
        }

        public void SaveConsoleOutputToFile(string filename, string subfolder)
        {
            filename = GenerateOutputFilePath(filename, subfolder, ".log");

            try
            {
                CreateDirecotoryIfNotExist(subfolder);

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

        public void SaveTextTofile(string text, string filename, string subfolder, string extension = "")
        {
            filename = GenerateOutputFilePath(filename, subfolder, extension);

            try
            {
                CreateDirecotoryIfNotExist(subfolder);

                File.WriteAllText(filename, text);
            }
            catch (Exception e)
            {
                _consoleHelper.WriteLineInRed(String.Format("An error occured while saving console output to file {0}", filename));
                _consoleHelper.WriteLineInRed(e.Message);
            }
        }

        public void SaveConsoleOutputToHtmlFile(string text, string filename, string subfolder)
        {
            try
            {
                SaveTextTofile(text, filename, subfolder, ".html");
            }
            catch (Exception e)
            {
                _consoleHelper.WriteLineInRed(String.Format("An error occured while saving console output to file {0}", filename));
                _consoleHelper.WriteLineInRed(e.Message);
            }
        }

        public void SaveWikiPagesToJsonFile(IEnumerable<WikiPage> pages, string filename, string subfolder)
        {
            filename = GenerateOutputFilePath(filename, subfolder, ".json");

            try
            {
                CreateDirecotoryIfNotExist(subfolder);

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

        private void CreateDirecotoryIfNotExist(string subfolder)
        {
            if (String.IsNullOrEmpty(subfolder))
                return;

            if (!Directory.Exists(subfolder))
                Directory.CreateDirectory(subfolder);
        }

        public List<WikiPage> LoadJsonFromFile(string filename)
        {
            try
            {
                string json = File.ReadAllText(filename);
                List<WikiPage> pages = JsonConvert.DeserializeObject<List<WikiPage>>(json);
                _consoleHelper.WriteLine(String.Format("Json load from: {0}", filename));
                return pages;
            }
            catch (Exception e)
            {
                _consoleHelper.WriteLineInRed(String.Format("An error occured while loading {0}", filename));
                _consoleHelper.WriteLineInRed(e.Message);
                return new List<WikiPage>();
            }
        }

        private string GenerateOutputFilePath(string filename, string subfolder, string extension)
        {
            if (String.IsNullOrEmpty(filename))
                filename = String.IsNullOrEmpty(subfolder) ? GenerateUniqueFileName(extension) : Path.Combine(subfolder, GenerateUniqueFileName(extension));
            return filename;
        }

        private string GenerateUniqueFileName(string extension)
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
