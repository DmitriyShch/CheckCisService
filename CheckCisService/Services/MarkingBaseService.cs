using CheckCisService.Models;
using Newtonsoft.Json;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("AptekarTests")]
namespace CheckCisService.Services
{
    /// <summary>
    /// Базовый сервис для работы с запросами проверки КИЗ.
    /// </summary>
    public class MarkingBaseService(ILogger<MarkingBaseService> logger)
    {
        /// <summary>
        /// Таймаут для выполнения запроса.
        /// </summary>
        protected int requestTimeout;

        /// <summary>
        /// Выполняет запрос проверки КИЗ и возвращает ответ сервиса.
        /// </summary>
        /// <param name="code">Код КИЗ.</param>
        /// <param name="requestMessage">HTTP-запрос.</param>
        /// <param name="httpClient">HTTP-клиент.</param>
        /// <returns>Ответ сервиса проверки КИЗ.</returns>
        protected async Task<CodeCheckResponse> CheckCode(string code,
            HttpRequestMessage requestMessage, HttpClient httpClient)
        {
            var cts = new CancellationTokenSource(requestTimeout);
            var startTime = DateTime.Now;
            var checkCisUri = requestMessage.RequestUri;
            try
            {
                var response = await httpClient.SendAsync(requestMessage, cts.Token);
                if (response == null)
                {
                    logger.LogWarning("Нет ответа от сервиса проверки. {CisUri}. " +
                        "response == null.", checkCisUri);
                    throw new HttpRequestException("Нет ответа от сервиса проверки");
                }
                response.EnsureSuccessStatusCode();
                var contentStr = await response.Content.ReadAsStringAsync();
                var codeCheckResponse = JsonConvert.DeserializeObject<CodeCheckResponse>(
                    contentStr);

                if (codeCheckResponse == null)
                {
                    logger.LogWarning("Нет содержания ответа от сервиса проверки. " +
                        "{CisUri}. response.Content: {ResponseBody}.", checkCisUri, contentStr);
                    throw new HttpRequestException($"Нет содержания ответа от сервиса проверки: " +
                        $"response: {contentStr}", inner: null, statusCode: response.StatusCode);
                }

                if (codeCheckResponse.Codes.Count == 0)
                {
                    logger.LogWarning("Нет информации по кодам проверки проверки. " +
                        "{CisUri}. response.Content: {ResponseBody}.", checkCisUri, contentStr);
                    throw new HttpRequestException($"Нет информации по кодам проверки проверки: " +
                        $"response: {contentStr}", inner: null, statusCode: response.StatusCode);
                }

                if (codeCheckResponse.Codes[0].Cis != code)
                {
                    logger.LogWarning("Полученный код {FactCis} отличается от проверяемого " +
                        "{ExpectedCis}.", codeCheckResponse.Codes[0].Cis, code);
                }

                codeCheckResponse.Host = requestMessage.RequestUri?.Host;
                codeCheckResponse.Duration = Convert.ToInt32((DateTime.Now - startTime)
                    .TotalMilliseconds);
                return codeCheckResponse;
            }
            catch (TaskCanceledException ex)
            {
                logger.LogError(ex, "[Timeout] Превышено время ожидания " +
                    "{requestTimeout} мсек для CDN {checkCisUri}",
                    requestTimeout, checkCisUri);
                throw new TimeoutException("Превышено время ожидания", ex);
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "[Error] Ошибка при запросе к CDN " +
                    "{checkCisUri}: {ex.Message}", checkCisUri, ex.Message);
                throw new HttpRequestException($"Ошибка при запросе к CDN {checkCisUri}. " +
                    $"{ex.Message}", ex, statusCode: ex.StatusCode);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "[Error] Ошибка при запросе к CDN " +
                    "{checkCisUri}: {ex.Message}", checkCisUri, ex.Message);
                throw new HttpRequestException($"Ошибка при запросе к CDN {checkCisUri}. " +
                    $"{ex.Message}", ex, statusCode: System.Net.HttpStatusCode.InternalServerError);
            }
        }
    }
}
