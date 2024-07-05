using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.IO;
using WikiRef.Common;
using WikiRef.Wiki;
using System.Text;
using System.Linq;

namespace WikiRef
{
    public interface IFileHelper
    {
        string GenerateUniqueFileName(string extension);
        List<string> LoadWhiteListFromJsonFile(string filename);
        WikiRefCache LoadWikiRefCacheFromJsonFile(string filename);
        void SaveConsoleOutputToHtmlFile();
        void SaveConsoleOutputToLogFile();
        void SaveTextToFile(string text, string filename, string extension);
        void SaveWikiRefCacheReferencesContentToTextFile(WikiRefCache cache);
        void SaveWikiRefCacheToJsonFile(WikiRefCache cache);
    }

    public class FileHelper : IFileHelper
    {
        protected IConsole _console;
        protected IAppConfiguration _config;

        public FileHelper(IAppConfiguration config, IConsole console)
        {
            _console = console;
            _config = config;
        }

        public void SaveConsoleOutputToLogFile()
        {
            if (_config != null && (_config.ConsoleOutputToDefaultLogFile || !String.IsNullOrEmpty(_config.ConsoleOutputToLogFile)))
            {
                string filename = _config.ConsoleOutputToDefaultLogFile ? String.Empty : _config.ConsoleOutputToLogFile;
                SaveTextToFile(_console.GetTextBuffer(), filename, ".log");
            }
        }

        public void SaveConsoleOutputToHtmlFile()
        {
            if (_config != null && (_config.ConsoleOutputToDefaultHtmlFile || !String.IsNullOrEmpty(_config.ConsoleOutputToHtmlFile)))
            {
                string filename = _config.ConsoleOutputToDefaultHtmlFile ? String.Empty : _config.ConsoleOutputToHtmlFile;
                SaveTextToFile(_console.GetHtml(), filename, ".html");
            }
        }

        public void SaveTextToFile(string text, string filename, string extension)
        {
            filename = String.IsNullOrEmpty(filename) ? GenerateUniqueFileName(extension) : filename;

            try
            {
                File.WriteAllText(filename, text);
            }
            catch (Exception e)
            {
                _console.WriteLineInRed($"An error occured while saving console output to file {filename}");
                _console.WriteLineInRed(e.Message);
            }
        }

        public List<string> LoadWhiteListFromJsonFile(string filename)
        {
            try
            {
                string json = File.ReadAllText(filename);
                List<string> whitelist = JsonConvert.DeserializeObject<List<string>>(json);
                _console.WriteLine(String.Format("Json load from: {0}", filename));
                return whitelist;
            }
            catch (Exception e)
            {
                _console.WriteLineInRed(String.Format("An error occured while loading {0}", filename));
                _console.WriteLineInRed(e.Message);
                return new List<string>();
            }
        }

        public WikiRefCache LoadWikiRefCacheFromJsonFile(string filename)
        {
            try
            {
                string json = File.ReadAllText(filename);
                WikiRefCache cache = JsonConvert.DeserializeObject<WikiRefCache>(json);
                _console.WriteLine(String.Format("Json load from: {0}", filename));
                return cache;
            }
            catch (Exception e)
            {
                _console.WriteLineInRed(String.Format("An error occured while loading {0}", filename));
                _console.WriteLineInRed(e.Message);
                return new WikiRefCache();
            }
        }

        public void SaveWikiRefCacheToJsonFile(WikiRefCache cache)
        {
            AppConfiguration config = (AppConfiguration)_config;
            if (config != null && (config.OutputJsonToDefaultFile || !String.IsNullOrEmpty(config.OutputJsonToFile)))
            {
                string filename = config.OutputJsonToDefaultFile ? String.Empty : config.OutputJsonToFile;
                try
                {
                    string output = JsonConvert.SerializeObject(cache, Formatting.Indented);
                    SaveTextToFile(output, filename, ".json");
                    _console.WriteLine(String.Format("Json saved to: {0}", filename));
                }
                catch (Exception e)
                {
                    _console.WriteLineInRed(String.Format("An error occured while saving console output to file {0}", filename));
                    _console.WriteLineInRed(e.Message);
                }
            }
        }

        public void SaveWikiRefCacheReferencesContentToTextFile(WikiRefCache cache)
        {
            if (_config != null && _config.ExportRefToTextFile)
            {
                StringBuilder builder = new StringBuilder();
                foreach (var reference in cache.Wiki.Namespaces.SelectMany(p => p.Pages).SelectMany(p => p.References).Select(r => r.Content))
                    builder.AppendLine(reference);
                SaveTextToFile(builder.ToString(), "", ".txt");
            }
        }

        public string GenerateUniqueFileName(string extension)
        {
            string filenameWithoutExtension = $"output_{DateTime.Now.ToString("yyyyMMddTHH-mm-ss")}";
            string filenameWithExtension = $"{filenameWithoutExtension}{extension}";

            while (File.Exists(filenameWithExtension))
            {
                filenameWithoutExtension += "_";
                filenameWithExtension = $"{filenameWithoutExtension}{extension}";
            }

            return filenameWithExtension;
        }
    }
}
