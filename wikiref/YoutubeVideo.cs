using System;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;

namespace WikiRef
{
    class YoutubeVideo
    {
        ConsoleHelper _consoleHelper;
        AppConfiguration _configuration;

        public string Url { get; private set; }
        public string UrlWithoutArguments { get; set; }
        public string Name { get; private set; }
        public string FileName { get; private set; }

        public YoutubeVideo(string url, ConsoleHelper consoleHelper, AppConfiguration configuration)
        {
            Url = url;
            
            _consoleHelper = consoleHelper;
            _configuration = configuration;

            RetreiveName();
            GenerateFileName();
            BuildUrlWihtoutArgument();
        }

        public void BuildUrlWihtoutArgument()
        {
            if (String.IsNullOrEmpty(Url))
                return;

            if (Url.Contains("?t"))            
                Url = Url.Split("?t")[0];
            
            else if (Url.Contains("&t"))            
                Url = Url.Split("&t")[0];            
        }

        public void RetreiveName()
        {
            bool tooManyRequest = true;
            while (tooManyRequest) {
                Thread.Sleep(5000);
                using (WebClient client = new WebClient())
                {
                    try
                    {
                        if (_configuration.Throttle != 0)
                            Thread.Sleep(1000 * _configuration.Throttle);

                        if (_configuration.Verbose)
                            _consoleHelper.WriteLineInGray(String.Format("Retreiving name for {0}", Url));

                        string pageContent = client.DownloadString(Url);
                        string urlfilterRegularExpression = "(<meta property=.og:title. content=.)(.*?)(.)([>])"; // match <meta name="title" content="TITLE"> in 4 group, title is in group 2
                        Regex linkParser = new Regex(urlfilterRegularExpression, RegexOptions.Compiled | RegexOptions.IgnoreCase);
                        var matches = linkParser.Matches(pageContent);

                        foreach (Match match in matches)
                            Name = HttpUtility.HtmlDecode(match.Groups[2].Value);

                        tooManyRequest = false;
                    }
                    catch (WebException ex)
                    {
                        HttpWebResponse response = (System.Net.HttpWebResponse)ex.Response;
                        if (response.StatusCode == HttpStatusCode.TooManyRequests)
                        {
                            _consoleHelper.WriteLineInRed(String.Format("URL: {0} - Erreur: {1} - Retry in {2}sec", Url, ex.Message, 5000));
                            Thread.Sleep(5000);
                        }
                        else
                            _consoleHelper.WriteLineInRed(String.Format("URL: {0} - Erreur: {1}", Url, ex.Message));
                    }
                    catch (Exception e)
                    {
                        _consoleHelper.WriteLineInRed(String.Format("URL: {0} - Erreur: {1}", Url, e.Message));
                    }
                } 
            }
        }

        private void GenerateFileName()
        {
            if (String.IsNullOrEmpty(Name))
                return;

            // Make video name valid for use as path
            var name = Name;
            foreach (char c in System.IO.Path.GetInvalidFileNameChars())
            {
                name = name.Replace(c, '_');
            }
            name = name.Replace(' ', '_');
            FileName = name;

            // Add youtube code as reference in case the reference doesn't contain file name, split on / then remove & and ? args
            // match:   https://youtu.be/SQuSdHIfuoU?t=37
            //          https://www.youtube.com/watch?v=nA0eTwsdhS8&t=772
            //          https://www.youtube.com/shorts/PElWZRmFwc4
            var youtubeName = Url.Split('/').ToList().Last();
            youtubeName = youtubeName.Split('?').ToList().First();
            youtubeName = youtubeName.Split('&').ToList().First();

            FileName = String.Format("{0}_{1}", name, youtubeName);
        }

        // test data
        //https://youtu.be/SQuSdHIfuoU?t=37
        //https://www.youtube.com/watch?v=nA0eTwsdhS8&t=772   
        //https://www.youtube.com/shorts/PElWZRmFwc4
        //https://youtube.com/shorts/dQw4w9WgXcQ?feature=share',
        //https://youtube.com/shorts/dQw4w9WgXcQ?feature=share,
        ////www.youtube-nocookie.com/embed/up_lNV-yoK4?rel=0,
        //http://www.youtube.com/user/Scobleizer#p/u/1/1p3vcRhsYGo,
        //http://www.youtube.com/watch?v=cKZDdG9FTKY&feature=channel,
        //http://www.youtube.com/watch?v=yZ-K7nCVnBI&playnext_from=TL&videos=osPknwzXEas&feature=sub,
        //http://www.youtube.com/ytscreeningroom?v=NRHVzbJVx8I,
        //http://www.youtube.com/user/SilkRoadTheatre#p/a/u/2/6dwqZw0j_jY,
        //http://youtu.be/6dwqZw0j_jY,
        //http://www.youtube.com/watch?v=6dwqZw0j_jY&feature=youtu.be,
        //http://youtu.be/afa-5HQHiAs,
        //http://www.youtube.com/user/Scobleizer#p/u/1/1p3vcRhsYGo?rel=0,
        //http://www.youtube.com/watch?v=cKZDdG9FTKY&feature=channel,
        //http://www.youtube.com/watch?v=yZ-K7nCVnBI&playnext_from=TL&videos=osPknwzXEas&feature=sub,
        //http://www.youtube.com/ytscreeningroom?v=NRHVzbJVx8I,
        //http://www.youtube.com/embed/nas1rJpm7wY?rel=0,
        //http://www.youtube.com/watch?v=peFZbP64dsU,
        //http://youtube.com/v/dQw4w9WgXcQ?feature=youtube_gdata_player,
        //http://youtube.com/vi/dQw4w9WgXcQ?feature=youtube_gdata_player,
        //http://youtube.com/?v=dQw4w9WgXcQ&feature=youtube_gdata_player,
        //http://www.youtube.com/watch?v=dQw4w9WgXcQ&feature=youtube_gdata_player,
        //http://youtube.com/?vi=dQw4w9WgXcQ&feature=youtube_gdata_player,
        //http://youtube.com/watch?v=dQw4w9WgXcQ&feature=youtube_gdata_player,
        //http://youtube.com/watch?vi=dQw4w9WgXcQ&feature=youtube_gdata_player,
        //http://youtu.be/dQw4w9WgXcQ?feature=youtube_gdata_player
        //Something<ref> Une video - date -  <nowiki>https://www.youtube.com/watch?v=2j3p_aDMTNg</nowiki></ref>
        //https://www.youtube.com/shorts/PElWZRmFwc4/test
    }
}
