using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using WikiRef.Data;
using WikiRef.Wiki;

namespace WikiRef.Report
{
    internal interface IReportBuilder
    {
        string BuildReport();
    }

    internal class ReportBuilder : IReportBuilder
    {
        WikiRefCache _cache;
        StringBuilder _buffer;

        public ReportBuilder(WikiRefCache cache)
        {
            _cache = cache;
            _buffer = new StringBuilder();
        }

        public string BuildReport()
        {
            _buffer.AppendLine($"'''Date: {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}'''");
            _buffer.AppendLine($"__NOTOC__");
            BuildOverviewSection();
            foreach (var ns in _cache.Wiki.Namespaces)
                BuildDetailSection(ns);
            BuildWhiteListSection();
            return _buffer.ToString();
        }

        private void BuildOverviewSection()
        {
            BuilSectionTitle("Overview");
            BuildOverviewSectionTableHeader();
            foreach (var page in _cache.Wiki.Namespaces.SelectMany(ns => ns.Pages))
                BuildOverviewSectionTableLine(page);
            BuildOverviewSectionTableFooter();
        }

        private void BuilSectionTitle(string name)
        {
            _buffer.AppendLine($"== {name} ==");
        }

        private void BuildOverviewSectionTableHeader()
        {
            _buffer.AppendLine("{| class=\"wikitable\"");
            _buffer.AppendLine(@"
            |+
            !Page
            !References
            !Citations
            !Whitelisted
            !Valid
            !Invalid
            !Status");
        }

        private void BuildOverviewSectionTableLine(Data.WikiPage page)
        {

            var reference = page.References.Count;
            var citation = page.References.Where(r => r.IsCitation).Count();
            var whitelisted = page.References.SelectMany(u => u.Urls).Where(u => u.SourceStatus == SourceStatus.WhiteListed).Count();
            var invalidSource = page.References.Where(r => !r.IsCitation).Where(r => r.Status == SourceStatus.Invalid).Count();
            var validSource = page.References.Where(r => !r.IsCitation).Where(r => r.Status != SourceStatus.Invalid).Count();
            string status = page.References.Where(r => !r.IsCitation).Where(r => r.Status == SourceStatus.Invalid).Count() == 0 ? " Valid" : "Invalid";
            string color = invalidSource > 0 ? "#f06130" : "#a7f030";
            _buffer.AppendLine($@"
                    |-
                    |[[{page.Name}]]
                    |{reference}
                    |{citation}                    
                    |{whitelisted}
                    |{validSource}
                    |{invalidSource}
                    |style=""background-color: {color}"" | {status}");
        }

        private void BuildOverviewSectionTableFooter()
        {
            _buffer.AppendLine("|}");
        }

        private void BuildDetailSection(WikiNamespace ns)
        {
            if (ns.Pages.Where(p => p.References.Any(r => r.Status == SourceStatus.Invalid)).Count() > 0)
            {
                BuilSectionTitle($"Error details - {ns.Name}");
                foreach (var page in ns.Pages.Where(p => p.References.Any(r => r.Status == SourceStatus.Invalid)))
                    BuildErrorDetail(page);
            }
        }

        private void BuildErrorDetail(Data.WikiPage page)
        {
            int i = 0;
            _buffer.AppendLine($"=== {page.Name} ===");
            foreach (var reference in page.References.Where(r => r.Status == SourceStatus.Invalid))
            {
                if (i > 0) _buffer.AppendLine("<hr />");
                _buffer.AppendLine($"<code><nowiki>{reference.Content}</nowiki></code>");
                i += 1;
            }
        }

        private void BuildWhiteListSection()
        {
            BuilSectionTitle("Whitelist");
            _buffer.AppendLine($"=== Whitelisted domains ===");
            BuildWhiteListedUrl();
            _buffer.AppendLine("<hr />");
            foreach (var pages in _cache.Wiki.Namespaces.SelectMany(ns => ns.Pages))
                BuildWhiteListDetail(pages);
        }

        private void BuildWhiteListedUrl()
        {
            foreach (var url in _cache.WhiteList)
                _buffer.AppendLine($" - {url}");
        }

        private void BuildWhiteListDetail(Data.WikiPage page)
        {
            IEnumerable<string> urls = page.References.SelectMany(u => u.Urls).Where(u => u.SourceStatus == SourceStatus.WhiteListed).Select(u => u.Url);
            if (urls.Count() == 0) return;
            _buffer.AppendLine($"=== {page.Name} ===");
            foreach (var url in urls.OrderBy(u => u))
                _buffer.AppendLine($" - {url}");
        }
    }
}
