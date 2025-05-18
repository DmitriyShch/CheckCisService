using CheckCisService.Config;
using CheckCisService.Exceptions;
using CheckCisService.Helpers;
using CheckCisService.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Text;
using static CheckCisService.Models.CdnListResponse;

namespace CheckCisService.Services
{
    /// <summary>
    /// Сервис для онлайн-проверки КИЗ.
    /// </summary>
    public class MarkingOnlineService : MarkingBaseService
    {
        private const string X_API_KEY_HEADER = "X-API-KEY";
        public const string APPLICATION_JSON = "application/json";
        private readonly HttpClient checkCodeHttpClient;
        private readonly OnlineCheckConfig config;
        private readonly string fiscalRegNumber;
        private readonly string apiKey;
        private readonly CheckCisUriResolver uriResolver;
        private readonly ILogger<MarkingOnlineService> logger;

        /// <summary>
        /// Текущий CDN-хост для проверки.
        /// </summary>
        public string? CdnHost
        {
            get => uriResolver.CdnHost;
            set => uriResolver.CdnHost = value;
        }

        public MarkingOnlineService(ILogger<MarkingOnlineService> logger,
            IOptions<MdlpConfig> mdlpConfig) : base(logger)
        {
            apiKey = mdlpConfig.Value.ApiKey;
            config = mdlpConfig.Value.OnlineService;
            fiscalRegNumber = mdlpConfig.Value.FiscalSerialNumber;
            this.logger = logger;
            checkCodeHttpClient = CreateHttpClient(keepAlive: true);
            requestTimeout = TimeSpan.FromMilliseconds(config.ReqTimeout);
            uriResolver = new CheckCisUriResolver(config.Host, config.UrlPrefix);
        }

        protected HttpClient CreateHttpClient(bool keepAlive = false)
        {
            var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add(X_API_KEY_HEADER, apiKey);
            httpClient.DefaultRequestHeaders.ConnectionClose = !keepAlive;
            return httpClient;
        }

        /// <summary>
        /// Получает список доступных CDN-хостов.
        /// </summary>
        /// <returns>Список CDN-хостов.</returns>
        public async Task<List<CdnHost>> GetCdnList()
        {
            using var client = CreateHttpClient(keepAlive: false);
            try
            {
                var cdnListResponse = await client.GetAsync(uriResolver.CdnInfoUri);
                var responseStr = await cdnListResponse.Content.ReadAsStringAsync();
                logger.LogDebug("GetCdnList response: {responseStr}", responseStr);
                cdnListResponse.EnsureSuccessStatusCode();
                var cdnHosts = JsonConvert.DeserializeObject<CdnListResponse>(responseStr)
                    ?.Hosts ?? [];
                return [.. cdnHosts];
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при запросе списка доступных узлов. " +
                    "{CdnInfoUri}", uriResolver.CdnInfoUri);
                throw new ServiceException($"Ошибка при запросе списка доступных узлов. " +
                    $"{uriResolver.CdnInfoUri}", ex);
            }
        }

        /// <summary>
        /// Получает время отклика CDN-хоста.
        /// </summary>
        /// <param name="host">Адрес CDN-хоста.</param>
        /// <returns>Время отклика в миллисекундах.</returns>
        public async Task<int?> GetHostResponseTime(string host)
        {
            using var client = CreateHttpClient(keepAlive: false);
            var startTime = DateTime.Now;
            var cts = new CancellationTokenSource(config.CheckHealthReqTimeout);
            uriResolver.CdnHost = host;
            try
            {
                var responseMessage = await client.GetAsync(uriResolver.HealthCheckUri, cts.Token);
                var responseTime = Convert.ToInt32((DateTime.Now - startTime).TotalMilliseconds);
                logger.LogDebug("HealthCheckAsync Status: {responseMessage.StatusCode}," +
                    " Content: {responseMessage.Content}", responseMessage.StatusCode,
                    responseMessage.Content.Json());
                responseMessage.EnsureSuccessStatusCode();
                return responseTime;
            }
            catch (TaskCanceledException exp)
            {
                logger.LogWarning(exp, "[Timeout] Превышено время ожидания " +
                    "{config.CheckHealthReqTimeout} мсек. " +
                    "Uri: {uriResolver.HealthCheckUri}. {exp.Message}",
                    config.CheckHealthReqTimeout, uriResolver.HealthCheckUri,
                    exp.Message);
                return null;
            }
            catch (Exception exp)
            {
                logger.LogError(exp, "Ошибка при запросе доступности сервиса проверки кизов." +
                    "Uri: {uriResolver.HealthCheckUri}. {exp.Message}",
                    uriResolver.HealthCheckUri, exp.Message);
                return null;
            }
        }

        /// <summary>
        /// Проверяет КИЗ через онлайн-сервис.
        /// </summary>
        /// <param name="code">Код КИЗ.</param>
        /// <returns>Ответ сервиса проверки КИЗ.</returns>
        public async Task<CodeCheckResponse> CheckCode(string code)
        {
            var requestBody = new
            {
                codes = new[] { code },
                fiscalDriveNumber = fiscalRegNumber
            };
            var content = new StringContent(requestBody.Json(), Encoding.UTF8, APPLICATION_JSON);
            HttpRequestMessage requestMessage = new(HttpMethod.Post, uriResolver.CodesCheckUri)
            {
                Content = content
            };
            var result = await CheckCode(code, requestMessage, checkCodeHttpClient);
            result.IsOnline = true;
            return result;
        }

        internal class CheckCisUriResolver(string baseHost, string apiPrefix)
        {
            public const string CDN_INFO = "cdn/info";
            public const string HEALTH_CHECK = "cdn/health/check";
            public const string CODES_CHECK = "codes/check";

            public string? CdnHost { get; set; }

            public Uri CdnInfoUri => new(new Uri(new Uri(baseHost), apiPrefix), CDN_INFO);

            public Uri HealthCheckUri => CdnHost == null
                ? throw new ArgumentNullException("Не задан адрес CDN хоста")
                : new(new Uri(new Uri(CdnHost), apiPrefix), HEALTH_CHECK);

            public Uri CodesCheckUri => CdnHost == null
                ? throw new ArgumentNullException("Не задан адрес CDN хоста")
                : new(new Uri(new Uri(CdnHost), apiPrefix), CODES_CHECK);
        }
    }
}
