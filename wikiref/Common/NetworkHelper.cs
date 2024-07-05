using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace WikiRef.Common
{
    public interface INetworkHelper
    {
        Task<string> GetContent(string url, bool cookieLess = false);
        Task<HttpStatusCode> GetStatus(string url);
        Task<string> GetYoutubeShortContent(string url);
    }

    public class NetworkHelper : INetworkHelper
    {
        private IConsole _console;

        private HttpClient _httpClient;
        private HttpClient _httpClientCookieless;
        Func<SocketsHttpConnectionContext, CancellationToken, ValueTask<Stream>> _ipv4ConnectionCallback;

        public NetworkHelper(IAppConfiguration config, IConsole console)
        {
            _console = console;
            if (config.Ipv4Only)
            {
                // source: https://www.meziantou.net/forcing-httpclient-to-use-ipv4-or-ipv6-addresses.htm
                _ipv4ConnectionCallback = async (context, cancellationToken) =>
                {
                    var entry = await Dns.GetHostEntryAsync(context.DnsEndPoint.Host, AddressFamily.InterNetwork, cancellationToken);

                    var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

                    socket.NoDelay = true;

                    try
                    {
                        await socket.ConnectAsync(entry.AddressList, context.DnsEndPoint.Port, cancellationToken);
                        return new NetworkStream(socket, ownsSocket: true);
                    }
                    catch
                    {
                        socket.Dispose();
                        throw;
                    }
                };

                _httpClient = new HttpClient(new SocketsHttpHandler()
                {
                    ConnectCallback = _ipv4ConnectionCallback
                });

                _httpClientCookieless = new HttpClient(new SocketsHttpHandler()
                {
                    UseCookies = false,
                    ConnectCallback = _ipv4ConnectionCallback
                });

            }
            else
            {
                _httpClient = new HttpClient();
                _httpClientCookieless = new HttpClient(new SocketsHttpHandler()
                {
                    UseCookies = false,
                });
            }

            ConfigureHttpClientHeaders(_httpClient);
            ConfigureHttpClientHeaders(_httpClientCookieless);

        }

        private void ConfigureHttpClientHeaders(HttpClient httpclient)
        {
            httpclient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/111.0");
            httpclient.DefaultRequestHeaders.Add("Accept", "*/*");
            httpclient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
            httpclient.DefaultRequestHeaders.Add("Connection", "keep-alive");
            httpclient.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
            httpclient.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
            httpclient.DefaultRequestHeaders.Add("Sec-Fetch-Site", "none");
            httpclient.DefaultRequestHeaders.Add("Sec-Fetch-Use", "?1");
            httpclient.DefaultRequestHeaders.Add("Sec-GPC", "1");
            httpclient.DefaultRequestHeaders.Add("TE", "trailers");
            httpclient.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
        }

        public async Task<HttpStatusCode> GetStatus(string url)
        {
            HttpStatusCode result = HttpStatusCode.NotFound;
            try
            {
                if (!url.Trim().StartsWith("https") && !url.Trim().StartsWith("http"))
                    url = "http://" + url;

                using HttpResponseMessage response = await _httpClient.GetAsync(url);
                return response.StatusCode;
            }
            catch (HttpRequestException httpException)
            {
                if (httpException.StatusCode == HttpStatusCode.TooManyRequests)
                    _console.WriteLineInRed($"URL: {url} - Error: {httpException.Message} - Too Many Request - Retry in 50 seconds");
                else
                    _console.WriteLineInRed($"URL: {url} - Error: {httpException.Message} - Status code: {httpException.StatusCode}");
                return result;
            }
            catch (Exception ex)
            {
                _console.WriteLineInRed($"URL: {url} - Error: {ex.Message}");
                return result;
            }
        }

        public async Task<string> GetContent(string url, bool cookieLess = false)
        {
            string result = String.Empty;
            try
            {
                if (!url.Trim().StartsWith("https") && !url.Trim().StartsWith("http"))
                    url = "https://" + url;

                using HttpResponseMessage response = cookieLess ? await _httpClientCookieless.GetAsync(url) : await _httpClient.GetAsync(url);
                using HttpContent content = response.Content;
                return await content.ReadAsStringAsync();
            }
            catch (HttpRequestException httpException)
            {
                if (httpException.StatusCode == HttpStatusCode.TooManyRequests)
                    _console.WriteLineInRed($"URL: {url} - Error: {httpException.Message} - Too Many Request - Retry in 50 seconds");
                else
                    _console.WriteLineInRed($"URL: {url} - Error: {httpException.Message} - Status code: {httpException.StatusCode}");
                return result;
            }
            catch (Exception ex)
            {
                _console.WriteLineInRed($"URL: {url} - Error: {ex.Message}");
                result = ex.Message;
                return result;
            }
        }

        public async Task<string> GetYoutubeShortContent(string url)
        {
            // required cookie to bypass "accept cookie screen" on ytb. Disabling cookie on the httpclient solve the problem, but create problems on other websites.
            _httpClient.DefaultRequestHeaders.Add("Cookie", "SOCS=CAISNQgDEitib3FfaWRlbnRpdHlmcm9udGVuZHVpc2VydmVyXzIwMjMwNzA0LjA1X3AwGgJlbiACGgYIgKudpQY; CONSENT=PENDING+658;");
            var result = await GetContent(url);
            _httpClient.DefaultRequestHeaders.Remove("Cookie");
            return result;
        }
    }
}
