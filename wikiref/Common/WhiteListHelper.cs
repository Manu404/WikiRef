using System;
using System.Collections.Generic;
using System.Linq;

namespace WikiRef.Common
{
    public class WhiteListHelper
    {
        public List<string> WhiteList { get; private set; }

        public WhiteListHelper(List<string> whiteList)
        {
            WhiteList = whiteList;
        }

        public bool CheckIfUrlIsWhiteListed(string url)
        {
            return WhiteList.Any(u => url.ToLower().StartsWith("https://" + u.ToLower(), StringComparison.InvariantCultureIgnoreCase))
            || WhiteList.Any(u => url.ToLower().StartsWith("http://" + u.ToLower(), StringComparison.InvariantCultureIgnoreCase))
            || WhiteList.Any(u => url.ToLower().StartsWith(u.ToLower(), StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
