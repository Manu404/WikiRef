using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace WikiRef
{
    internal class NetworkHelper
    {
        private ConsoleHelper _console;

        private HttpClient _httpClient;
        private HttpClient _httpClientCookieless;
        Func<SocketsHttpConnectionContext, CancellationToken, ValueTask<Stream>> _ipv4ConnetionCallback;

        public NetworkHelper(ConsoleHelper console)
        {
            // source: https://www.meziantou.net/forcing-httpclient-to-use-ipv4-or-ipv6-addresses.htm
            _ipv4ConnetionCallback = async (context, cancellationToken) =>
            {
                // Use DNS to look up the IP addresses of the target host:
                // - IP v4: AddressFamily.InterNetwork
                // - IP v6: AddressFamily.InterNetworkV6
                // - IP v4 or IP v6: AddressFamily.Unspecified
                // note: this method throws a SocketException when there is no IP address for the host
                var entry = await Dns.GetHostEntryAsync(context.DnsEndPoint.Host, AddressFamily.InterNetwork, cancellationToken);

                // Open the connection to the target host/port
                var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);

                // Turn off Nagle's algorithm since it degrades performance in most HttpClient scenarios.
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
                ConnectCallback = _ipv4ConnetionCallback
            });

            _httpClientCookieless = new HttpClient(new SocketsHttpHandler()
            {
                UseCookies = false,
                ConnectCallback = _ipv4ConnetionCallback
            });

            ConfigureHttpClientHeaders(_httpClient);
            ConfigureHttpClientHeaders(_httpClientCookieless);

            _console = console;
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

        public async Task<string> GetContent(string url, bool cookieLess = false)
        {
            string result = String.Empty;
            try
            {
                using HttpResponseMessage response = cookieLess ? await _httpClientCookieless.GetAsync(url) : await _httpClient.GetAsync(url);
                using HttpContent content = response.Content;
                return await content.ReadAsStringAsync();
            }
            catch(HttpRequestException httpException)
            {
                if (httpException.StatusCode == HttpStatusCode.TooManyRequests)
                    _console.WriteLineInRed($"URL: {url} - Erreur: {httpException.Message} - Too Many Request - Retry in 50 seconds");
                else
                    _console.WriteLineInRed($"URL: {url} - Erreur: {httpException.Message} - Status code: {httpException.StatusCode}");
                return result;
            }
            catch(Exception ex)
            {
                _console.WriteLineInRed($"URL: {url} - Erreur: {ex.Message}");
                result = ex.Message;
            }
            return result;
        }

        public async Task<string> GetYoutubeShortContent(string url)
        {
            // required cookie to bypass "accept cookie screen". Disabling cookie on the httpclient solve the solution, but create problems on other websites.
            // Validity of current cookies
            // CONSENT: Mon, 31 Mar 2025 08:49:22 GMT
            // SOCS: Tue, 30 Apr 2024 08:49:25 GMT
            _httpClient.DefaultRequestHeaders.Add("Cookie", "SOCS=CAISNQgDEitib3FfaWRlbnRpdHlmcm9udGVuZHVpc2VydmVyXzIwMjMwMzI4LjA1X3AwGgJlbiACGgYIgOidoQY; CONSENT=PENDING+823;");
            var result = await GetContent(url);
            _httpClient.DefaultRequestHeaders.Remove("Cookie");
            return result;
        }

        public async Task<HttpStatusCode> GetStatus(string url)
        {
            HttpStatusCode result = HttpStatusCode.NotFound;
            try
            {
                using HttpResponseMessage response = await _httpClient.GetAsync(url);
                return response.StatusCode;
            }
            catch (HttpRequestException httpException)
            {
                if (httpException.StatusCode == HttpStatusCode.TooManyRequests)
                    _console.WriteLineInRed($"URL: {url} - Erreur: {httpException.Message} - Too Many Request - Retry in 50 seconds");
                else
                    _console.WriteLineInRed($"URL: {url} - Erreur: {httpException.Message} - Status code: {httpException.StatusCode}");
            }
            catch (Exception ex)
            {
                _console.WriteLineInRed($"URL: {url} - Erreur: {ex.Message}");
            }
            return result;
        }
    }
}
