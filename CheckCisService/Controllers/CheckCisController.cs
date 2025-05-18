using CheckCisService.Helpers;
using CheckCisService.Services;
using Microsoft.AspNetCore.Mvc;

namespace CheckCisService.Controllers
{
    /// <summary>
    /// ���������� ��� �������� ��� (����� ������������� ������).
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class CheckCisController(
        ILogger<CheckCisController> logger,
        MarkingApiService markingApiService
        ) : ControllerBase
    {
        /// <summary>
        /// ������� ������� �������� ���.
        /// </summary>
        public enum CheckCisServiceStatus
        {
            /// <summary>
            /// �������� ������ �������.
            /// </summary>
            Ok = 0,
            /// <summary>
            /// ������ ������-��������.
            /// </summary>
            OnlineCheckFailed = 1,
            /// <summary>
            /// ������ ������-��������.
            /// </summary>
            OfflineCheckFailed = 2,
            /// <summary>
            /// ������ ����� ��������.
            /// </summary>
            AllChecksFailed = 3,
        }

        /// <summary>
        /// �������� ������ ������� �������� ���.
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
        /// ��������� ��� �� �������� ����������.
        /// </summary>
        /// <param name="cis">
        /// ��� ������������� ����� (���).
        /// </param>
        /// <param name="fiscalSerialNumber">
        /// ���������� �������� ����� (�����������).
        /// </param>
        /// <returns>
        /// ��������� �������� ���.
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
        /// ����� �� ������ ������� �������.
        /// </summary>
        public class GetStatusResponse
        {
            /// <summary>
            /// ��� �������.
            /// </summary>
            public required int StatusCode { get; init; }
            /// <summary>
            /// ������������ �������.
            /// </summary>
            public required string StatusName { get; init; }
        }
    }
}
