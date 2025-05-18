using CheckCisService.Helpers;
using CheckCisService.Services;
using Microsoft.AspNetCore.Mvc;

namespace CheckCisService.Controllers
{
    /// <summary>
    /// Контроллер для проверки КИЗ (кодов идентификации знаков).
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class CheckCisController(
        ILogger<CheckCisController> logger,
        MarkingApiService markingApiService
        ) : ControllerBase
    {
        /// <summary>
        /// Статусы сервиса проверки КИЗ.
        /// </summary>
        public enum CheckCisServiceStatus
        {
            /// <summary>
            /// Проверка прошла успешно.
            /// </summary>
            Ok = 0,
            /// <summary>
            /// Ошибка онлайн-проверки.
            /// </summary>
            OnlineCheckFailed = 1,
            /// <summary>
            /// Ошибка офлайн-проверки.
            /// </summary>
            OfflineCheckFailed = 2,
            /// <summary>
            /// Ошибка обеих проверок.
            /// </summary>
            AllChecksFailed = 3,
        }

        /// <summary>
        /// Получить статус сервиса проверки КИЗ.
        /// </summary>
        [HttpGet("Status")]
        public IActionResult GetStatus()
        {
            var status = CheckCisServiceStatus.OnlineCheckFailed;
            logger.LogDebug("GetStatus");
            var result = new GetStatusResponse()
            {
                StatusCode = (int)status,
                StatusName = status.ToString(),
            };
            return new OkObjectResult(result);
        }

        /// <summary>
        /// Проверить КИЗ по заданным параметрам.
        /// </summary>
        /// <param name="cis">
        /// Код идентификации знака (КИЗ).
        /// </param>
        /// <param name="fiscalSerialNumber">
        /// Фискальный серийный номер (опционально).
        /// </param>
        /// <returns>
        /// Результат проверки КИЗ.
        /// </returns>
        [HttpGet("CheckCis")]
        public async Task<IActionResult> CheckCis(string cis, string? fiscalSerialNumber)
        {
            try
            {
                var result = await markingApiService.CheckCis(cis, fiscalSerialNumber);
                logger.LogInformation("CheckCisController.CheckCis. " +
                    "cis: {cis}, Status: {Status}", cis, result?.Status);
                logger.LogDebug("CheckCisController.CheckCis. " +
                    "cis: {cis}, Result: {Result}", cis, result.Json());
                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "CheckCisController.CheckCis. " +
                    "cis: {cis} Message: {Message}", cis, ex.Message);
                return new StatusCodeResult(StatusCodes.Status500InternalServerError);
            }
        }

        /// <summary>
        /// Ответ на запрос статуса сервиса.
        /// </summary>
        public class GetStatusResponse
        {
            /// <summary>
            /// Код статуса.
            /// </summary>
            public required int StatusCode { get; init; }
            /// <summary>
            /// Наименование статуса.
            /// </summary>
            public required string StatusName { get; init; }
        }
    }
}
