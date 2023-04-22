using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using WikiRef.Commons;
using WikiRef.Wiki;

namespace WikiRef
{
    public class FileHelper : WikiRef.Commons.FileHelper
    {
        private ConsoleHelper _consoleHelper;

        public FileHelper(ConsoleHelper consoleHelper) : base(consoleHelper)
        {
            _consoleHelper = consoleHelper;
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

        public List<string> LoadWhitelistFromJsonFile(string filename)
        {
            try
            {
                string json = File.ReadAllText(filename);
                List<string> whitelist = JsonConvert.DeserializeObject<List<string>>(json);
                _consoleHelper.WriteLine(String.Format("Json load from: {0}", filename));
                return whitelist;
            }
            catch (Exception e)
            {
                _consoleHelper.WriteLineInRed(String.Format("An error occured while loading {0}", filename));
                _consoleHelper.WriteLineInRed(e.Message);
                return new List<string>();
            }
        }

        public List<WikiPage> LoadWikiPagesFromJsonFile(string filename)
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
