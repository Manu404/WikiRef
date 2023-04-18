using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using WikiRef.Commons;
using WikiRef.Wiki;

namespace WikiRef
{
    class FileHelper : WikiRef.Commons.FileHelper
    {
        private ConsoleHelper _consoleHelper;
        private AppConfiguration _conf;

        public FileHelper(ConsoleHelper consoleHelper, AppConfiguration conf) : base(consoleHelper)
        {
            _consoleHelper = consoleHelper;
            _conf = conf;
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
    }
}
