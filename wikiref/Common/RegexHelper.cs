using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace WikiRef
{
    public class RegexHelper
    {
        public Regex ExtractReferenceREgex { get; private set; }
        public Regex ExtractUrlFromReferenceRegex { get; private set; }
        public Regex ExtractYoutubeVideoNameFromPageRegex { get; private set; }
        public Regex ExtractYoutubeUrlFromEmbededVideoRegex { get; private set; }
        public Regex YoutubeVideoIdFromUrlRegex { get; private set; }

        public RegexHelper()
        {
            BuildRegex();
        }

        private void BuildRegex()
        {
            string referenceContainingUrlExpression = @"([<]( *)(ref)( |.*?)([>])).*?(?:https?|www)?.*?([<]( *?)(/ref)( *?)[>])"; // egex developped with regex101, regex and the texting datas available heree: https://regex101.com/r/1SYr6f/1
            ExtractReferenceREgex = new Regex(referenceContainingUrlExpression, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            string urlFilterExpression = @"\b(?<url>(https?:.//?|www\.).*?)(?:</ref>|[ ])"; // regex developped with regex101, regex and the texting datas available heree: https://regex101.com/r/pQb3hs/1 
                                                                                        // It includes what can be considered "errors", but that that allow to detect malformed url like nowiki or multiple url referebces
            ExtractUrlFromReferenceRegex = new Regex(urlFilterExpression, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            string titleFilterExpression = @"(<title>)(?<name>.*?)(</title>)"; // regex developped with regex101, regex and the texting datas available heree: https://regex101.com/r/HOb95o/1
            ExtractYoutubeVideoNameFromPageRegex = new Regex(titleFilterExpression, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            string embededYoutubeUrlExpression = "(<a href=\")(?<url>.*?)(\")"; // regex developped with regex101, regex and the texting datas available heree: s https://regex101.com/r/aqcAnR/1
            ExtractYoutubeUrlFromEmbededVideoRegex = new Regex(embededYoutubeUrlExpression, RegexOptions.Compiled | RegexOptions.IgnoreCase);

            string youtubeUrlVideoIdFilder = @"(?<host>.*/)(?<watch>.*v=)?(?<videoid>[a-zA-Z0-9-_]+)"; // regex developped with regex101, regex and the texting datas available heree:  https://regex101.com/r/0tLwmD/1
            YoutubeVideoIdFromUrlRegex = new Regex(youtubeUrlVideoIdFilder, RegexOptions.Compiled | RegexOptions.IgnoreCase);
        }
    }
}
