using System.Text.RegularExpressions;

namespace WikiRef.Common
{
    public interface IRegexHelper
    {
        Regex ExtractChannelNameRegex { get; }
        Regex ExtractDomainRegex { get; }
        Regex ExtractMetaAndUrlRegex { get; }
        Regex ExtractReferenceFromPageRegex { get; }
        Regex ExtractUrlFromReferenceRegex { get; }
        Regex ExtractUrlTimeCodeRegex { get; }
        Regex ExtractYoutubeIdFromFileNameRegex { get; }
        Regex ExtractYoutubeUrlFromEmbeddedVideoRegex { get; }
        Regex ExtractYoutubeVideoIdFromUrlRegex { get; }
        Regex ExtractYoutubeVideoNameFromPageRegex { get; }
    }

    public class RegexHelper : IRegexHelper
    {
        public Regex ExtractReferenceFromPageRegex { get; private set; }
        public Regex ExtractUrlFromReferenceRegex { get; private set; }
        public Regex ExtractYoutubeVideoNameFromPageRegex { get; private set; }
        public Regex ExtractYoutubeUrlFromEmbeddedVideoRegex { get; private set; }
        public Regex ExtractYoutubeVideoIdFromUrlRegex { get; private set; }
        public Regex ExtractMetaAndUrlRegex { get; private set; }
        public Regex ExtractChannelNameRegex { get; private set; }
        public Regex ExtractUrlTimeCodeRegex { get; private set; }
        public Regex ExtractDomainRegex { get; private set; }
        public Regex ExtractYoutubeIdFromFileNameRegex { get; private set; }

        public RegexHelper()
        {
            BuildRegex();
        }


        private void BuildRegex()
        {
            string referenceContainingUrlExpression = @"([<]( *)(ref)( |.*?)([>])).*?(?:https?|www)?.*?(([<]( *?)(/ref)( *?)[>]?))"; // regex developped with regex101, regex and the test datas available heree: https://regex101.com/r/oqL42I/1
            ExtractReferenceFromPageRegex = new Regex(referenceContainingUrlExpression, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            string urlFilterExpression = @"\b(?<url>(https?:.//?|www\.).*?)(?:</ref>|[,]|[\]]|[ ]|[ ]|[<]|[\|])"; // regex developped with regex101, regex and the test datas available heree: https://regex101.com/r/BdZBzu/1
            ExtractUrlFromReferenceRegex = new Regex(urlFilterExpression, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            string titleFilterExpression = @"(<title>)(?<name>.*?)(</title>)"; // regex developped with regex101, regex and the test datas available heree: https://regex101.com/r/HOb95o/1
            ExtractYoutubeVideoNameFromPageRegex = new Regex(titleFilterExpression, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            string embeddedYoutubeUrlExpression = "(<a href=\")(?<url>.*?)(\")"; // regex developped with regex101, regex and the test datas available heree: s https://regex101.com/r/aqcAnR/1
            ExtractYoutubeUrlFromEmbeddedVideoRegex = new Regex(embeddedYoutubeUrlExpression, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            string youtubeUrlVideoIdFilder = @"(?<host>.*/)(?<watch>.*v=)?(?<videoid>[a-zA-Z0-9-_]+)"; // regex developped with regex101, regex and the test datas available heree:  https://regex101.com/r/xYS9aX/1
            ExtractYoutubeVideoIdFromUrlRegex = new Regex(youtubeUrlVideoIdFilder, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            string extractMetaAndUrl = @"([<]( *)(ref)( |.*)([>]))(?<meta>.*?)(?<url>http.*?)?([<]( *)(/ref)( *)[>])"; // regex developped with regex101, regex and the test datas available heree:  https://regex101.com/r/Oo9JR2/1
            ExtractMetaAndUrlRegex = new Regex(extractMetaAndUrl, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            string extractChannelName = @"(<link itemprop=name content=)(?<name>.*?)(>)"; // regex developped with regex101, regex and the test datas available heree: https://regex101.com/r/gQ945i/1
            ExtractChannelNameRegex = new Regex(extractChannelName, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            string extractUrlTimeCode = @"(t=)(?<time>[0-9]*)(&|s)?"; // regex developped with regex101, regex and the test datas available heree: https://regex101.com/r/Hr1T8I/1
            ExtractUrlTimeCodeRegex = new Regex(extractUrlTimeCode, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            string extractDomain = @"(https?:\/\/)(?<domain>.*?)(\/)"; // regex developped with regex101, regex and the test datas available heree: https://regex101.com/r/2Q9N1K/1
            ExtractDomainRegex = new Regex(extractDomain, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            string extractYoutubeIdFromFileName = @"(\[((?<id>.*?)\]))"; // regex developped with regex101, regex and the test datas available heree: https://regex101.com/r/2Q9N1K/1
            ExtractYoutubeIdFromFileNameRegex = new Regex(extractYoutubeIdFromFileName, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }
    }
}
