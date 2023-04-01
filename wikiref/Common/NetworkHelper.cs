using System;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace WikiRef
{
    internal class NetworkHelper
    {
        private HttpClient _httpClient;
        private ConsoleHelper _console;
        public NetworkHelper(ConsoleHelper console)
        {
            _httpClient = new HttpClient(new SocketsHttpHandler()
            {
                ConnectCallback = async (context, cancellationToken) =>
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

                        // If you want to choose a specific IP address to connect to the server
                        // await socket.ConnectAsync(
                        //    entry.AddressList[Random.Shared.Next(0, entry.AddressList.Length)],
                        //    context.DnsEndPoint.Port, cancellationToken);

                        // Return the NetworkStream to the caller
                        return new NetworkStream(socket, ownsSocket: true);
                    }
                    catch
                    {
                        socket.Dispose();
                        throw;
                    }
                }
            });

            ConfigureHttpClientHeaders();

            _console = console;
        }

        private void ConfigureHttpClientHeaders()
        {
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:109.0) Gecko/20100101 Firefox/111.0");
            _httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
            _httpClient.DefaultRequestHeaders.Add("Accept-Language", "en-US,en;q=0.5");
            _httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
            _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Dest", "document");
            _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Mode", "navigate");
            _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Site", "none");
            _httpClient.DefaultRequestHeaders.Add("Sec-Fetch-Use", "?1");
            _httpClient.DefaultRequestHeaders.Add("Sec-GPC", "1");
            _httpClient.DefaultRequestHeaders.Add("TE", "trailers");
            _httpClient.DefaultRequestHeaders.Add("Upgrade-Insecure-Requests", "1");
        }

        public async Task<string> GetContent(string url)
        {
            string result = String.Empty;
            try
            {
                using HttpResponseMessage response = await _httpClient.GetAsync(url);
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

        public async Task<string> GetYoutubeContent(string url)
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
