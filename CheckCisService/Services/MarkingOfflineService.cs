using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text;
using CheckCisService.Config;
using CheckCisService.Exceptions;
using CheckCisService.Helpers;
using CheckCisService.Models;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

[assembly: InternalsVisibleTo("CheckCisServiceTests")]
namespace CheckCisService.Services
{
    /// <summary>
    /// Сервис для офлайн-проверки КИЗ.
    /// </summary>
    public class MarkingOfflineService : MarkingBaseService
    {
        /// <summary>
        /// Формат для передачи данных в формате JSON.
        /// </summary>
        public const string APPLICATION_JSON = "application/json";
        public const string X_CLIENT_ID = "X-ClientId";
        private readonly HttpClient checkCodeHttpClient;
        private readonly OfflineCheckConfig config;
        private readonly string fiscalRegNumber;
        private readonly string apiKey;
        private readonly CheckCisUriResolver uriResolver;
        private readonly ILogger<MarkingOfflineService> logger;

        public MarkingOfflineService(ILogger<MarkingOfflineService> logger,
            IOptions<MdlpConfig> mdlpConfig) : base(logger)
        {
            apiKey = mdlpConfig.Value.ApiKey;
            config = mdlpConfig.Value.OfflineService;
            fiscalRegNumber = mdlpConfig.Value.FiscalSerialNumber;
            this.logger = logger;
            checkCodeHttpClient = CreateHttpClient(keepAlive: true);
            requestTimeout = config.ReqTimeout;
            uriResolver = new(config.Host, config.UrlPrefix);
        }

        protected HttpClient CreateHttpClient(bool keepAlive = false)
        {
            var httpClient = new HttpClient();
            MakeBasicAuth(httpClient, config.Login, config.Pwd);
            if (fiscalRegNumber != null)
                httpClient.DefaultRequestHeaders.Add(X_CLIENT_ID, fiscalRegNumber);
            httpClient.DefaultRequestHeaders.ConnectionClose = !keepAlive;
            return httpClient;
        }

        /// <summary>
        /// Получает статус локального модуля маркировки.
        /// </summary>
        /// <returns>Статус модуля.</returns>
        public async Task<MarkingModuleStatusDto?> GetStatus()
        {
            using var client = checkCodeHttpClient;
            var cts = new CancellationTokenSource(config.ReqTimeout);
            try
            {
                var response = await client.GetAsync(uriResolver.StatusUri, cts.Token);
                response.EnsureSuccessStatusCode();
                var txt = await response.Content.ReadAsStringAsync();
                var markingModuleStatusDto =
                    JsonConvert.DeserializeObject<MarkingModuleStatusDto>(txt);
                return markingModuleStatusDto;
            }
            catch (TaskCanceledException ex)
            {
                logger.LogError(ex, "[Timeout] Превышено время ожидания " +
                    "{requestTimeout} мсек для StatusUri {statusUri}",
                    config.ReqTimeout, uriResolver.StatusUri);
                throw new TimeoutException("Превышено время ожидания", ex);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при запросе статуса локального модуля. {InitUri}",
                    uriResolver.InitUri);
                throw new ServiceException("Ошибка при запросе статуса локального модуля", ex);
            }
        }

        /// <summary>
        /// Инициализирует локальную БД.
        /// </summary>
        /// <returns>True, если инициализация прошла успешно.</returns>
        public async Task<bool> InitDb()
        {
            logger.LogDebug("Start InitDb. {InitUri}. Token: {ApiKey}",
                uriResolver.InitUri, apiKey);
            if (string.IsNullOrEmpty(apiKey))
                throw new ArgumentException("Не задан токен доступа");
            var requestBody = new { token = apiKey };
            using var client = CreateHttpClient();
            try
            {
                var startTime = DateTime.Now;
                var content = new StringContent(requestBody.Json(),
                    Encoding.UTF8, APPLICATION_JSON);
                var response = await client.PostAsync(uriResolver.InitUri, content);
                var duration = (DateTime.Now - startTime).TotalMilliseconds;
                response.EnsureSuccessStatusCode();
                logger.LogInformation("Успешно выполнена инициализация локальной БД. " +
                    "Время: {Duration} мс.", duration);
                return true;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Ошибка при инициализации локальной БД. {InitUri}",
                    uriResolver.InitUri);
                return false;
            }
        }

        public static void MakeBasicAuth(HttpClient httpClient, string login, string passw)
        {
            var creds = $"{login}:{passw}";
            var base64String = Convert.ToBase64String(Encoding.ASCII.GetBytes(creds));
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", base64String);
        }

        /// <summary>
        /// Проверяет КИЗ через офлайн-сервис.
        /// </summary>
        /// <param name="cis">Код КИЗ.</param>
        /// <returns>Ответ сервиса проверки КИЗ.</returns>
        public async Task<CodeCheckResponse> CheckCis(string cis)
        {
            HttpRequestMessage requestMessage = new(HttpMethod.Get,
                uriResolver.CodesCheckUri(cis));
            var result = await CheckCode(cis, requestMessage, checkCodeHttpClient);
            result.IsOnline = false;
            return result;
        }

        internal static bool ParseCheckedCode(
          CodeCheckResponse.CheckedCode checkedCode, out string? description)
        {
            description = null;
            if (checkedCode.IsBlocked == true)
                description = "Продажа заблокирована";
            return string.IsNullOrEmpty(description);
        }

        internal class CheckCisUriResolver(string baseHost, string apiPrefix)
        {
            public const string CHECK_CIS = "cis/check?cis=";
            public const string INIT = "init";
            public const string STATUS = "status";
            private readonly Uri baseUri = new(new Uri(baseHost),
                apiPrefix.EndsWith('/') ? apiPrefix : apiPrefix + '/');
            public string? CdnHost { get; set; }
            public Uri CodesCheckUri(string cis) => new(baseUri, $"{CHECK_CIS}{cis}");
            public Uri InitUri => new(baseUri, INIT);
            public Uri StatusUri => new(baseUri, STATUS);
        }
    }
}
