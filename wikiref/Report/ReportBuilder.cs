using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using WikiRef.Commons;
using WikiRef.Commons.Data;
using WikiRef.Wiki;

namespace WikiRef.Report
{
    internal class ReportBuilder
    {
        WikiRefCache _cache;

        public ReportBuilder(WikiRefCache cache) 
        { 
            _cache = cache;
        }

        public string BuildReport()
        {
            StringBuilder builder = new StringBuilder();
            builder.AppendLine($"'''Date de génération du rapport: {DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss")}'''");
            builder.AppendLine($"__NOTOC__");
            BuildOverview(builder);
            BuildDetailSection(builder);
            BuildwhitelistSection(builder);
            return builder.ToString();
        }

        private void BuildOverview(StringBuilder builder)
        {
            AddSectionTitle(builder, "Overview");
            BuildPageTableHeader(builder);
            foreach(var page in _cache.WikiPages)
                BuildLine(builder, page);
            BuildPageTableFooter(builder);
        }

        private void AddSectionTitle(StringBuilder builder, string name)
        {
            builder.AppendLine($"== {name} ==");
        }

        private void BuildPageTableHeader(StringBuilder builder)
        {
            builder.AppendLine("{| class=\"wikitable\"");
            builder.AppendLine(@"
            |+
            !Page
            !References
            !Citations
            !Whitelisted
            !Valid
            !Invalid
            !Status".Trim());
        }

        private void BuildLine(StringBuilder builder, WikiPage page) 
        {
            var reference = page.References.Count;
            var citation = page.References.Where(r => r.IsCitation).Count();
            var whitelisted = page.References.SelectMany(u => u.Urls).Where(u => u.SourceStatus == SourceStatus.WhiteListed).Count();
            var invalidSource = page.References.Where(r => !r.IsCitation).Where(r => r.Status == SourceStatus.Invalid).Count();
            var validSource = page.References.Where(r => !r.IsCitation).Where(r => r.Status != SourceStatus.Invalid).Count();
            string status = page.References.Where(r => !r.IsCitation).Where(r => r.Status == SourceStatus.Invalid).Count() == 0 ? " Valid" : "Invalid";
            string color = invalidSource > 0 ? "#f06130" : "#a7f030"; 
            builder.AppendLine($@"
                    |-
                    |[[{page.Name}]]
                    |{reference}
                    |{citation}                    
                    |{whitelisted}
                    |{validSource}
                    |{invalidSource}
                    |style=""background-color: {color}"" | {status}".Trim());
        }

        private void BuildPageTableFooter(StringBuilder builder)
        {
            builder.AppendLine("|}");
        }

        private void BuildDetailSection(StringBuilder builder)
        {
            AddSectionTitle(builder, "Error details");
            if (_cache.WikiPages.Where(p => p.References.Any(r => r.Status == SourceStatus.Invalid)).Count() == 0)
                builder.AppendLine("No errors found in the wiki, everything seems alright 👌");
            else
                foreach (var page in _cache.WikiPages.Where(p => p.References.Any(r => r.Status == SourceStatus.Invalid)))
                    BuildeErrorDetail(builder, page);
        }

        private void BuildeErrorDetail(StringBuilder builder, WikiPage page)
        {
            int i = 0;
            builder.AppendLine($"=== {page.Name} ===");
            foreach (var reference in page.References.Where(r => r.Status == SourceStatus.Invalid))
            {
                if (i > 0) builder.AppendLine("<hr />");
                builder.AppendLine($"<code><nowiki>{reference.Content}</nowiki></code>");
                i += 1;
            }
        }

        private void BuildwhitelistSection(StringBuilder builder)
        {
            AddSectionTitle(builder, "Whitelist");
            builder.AppendLine($"=== Whitelisted domains ===");
            BuildWhitelistedUrl(builder);
            builder.AppendLine("<hr />");
            foreach (var page in _cache.WikiPages)
                BuildeWhitelistDetail(builder, page);
        }

        private void BuildWhitelistedUrl(StringBuilder builder)
        {
            foreach (var url in _cache.WhiteList)
                builder.AppendLine($" - {url}");
        }

        private void BuildeWhitelistDetail(StringBuilder builder, WikiPage page)
        {
            IEnumerable<string> urls = page.References.SelectMany(u => u.Urls).Where(u => u.SourceStatus == SourceStatus.WhiteListed).Select(u => u.Url);
            if (urls.Count() == 0) return;
            builder.AppendLine($"=== {page.Name} ===");
            foreach (var url in urls.OrderBy(u => u))            
                builder.AppendLine($" - {url}");
        }
    }
}
