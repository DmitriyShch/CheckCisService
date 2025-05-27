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
        /// Получить статус сервиса проверки КИЗ.
        /// </summary>
        [HttpGet("Status")]
        public async Task<IActionResult> GetStatus()
        {
            logger.LogDebug("GetStatus Begin");
            var status = await markingApiService.GetStatus();
            logger.LogDebug("GetStatus {status}", status);
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
        [HttpGet("CheckCisHistory")]
        public IActionResult GetCheckCisHistory(DateTime minDate, DateTime maxDate,
            string? fiscalSerialNumber)
        {
            try
            {
                var result = markingApiService.GetCheckCisHistory
                    (minDate, maxDate, fiscalSerialNumber);
                logger.LogInformation("CheckCisController.GetCheckCisHistory. " +
                    "minDate: {minDate}, maxDate: {maxDate}, " +
                    "fiscalSerialNumber: {fiscalSerialNumber}. Found {count} rows.", minDate,
                    maxDate, fiscalSerialNumber, result.Count);
                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "CheckCisController.GetCheckCisHistory. minDate: {minDate}, " +
                    "maxDate: {maxDate}, fiscalSerialNumber: {fiscalSerialNumber}, " +
                    "Message: {Message}", minDate, maxDate, fiscalSerialNumber, ex.Message);
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
