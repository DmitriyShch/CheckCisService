using CheckCisService.Config;
using CheckCisService.Exceptions;
using CheckCisService.Helpers;
using CheckCisService.Models;
using CheckCisService.Models.Enums;
using CheckCisService.Repositories;
using Microsoft.Extensions.Options;
using System.Net;
using static CheckCisService.Models.CdnListResponse;

namespace CheckCisService.Services
{
    /// <summary>
    /// Сервис для проверки КИЗ через онлайн и офлайн сервисы, а также ведения логов.
    /// </summary>
    public class MarkingApiService(
        IOptions<MdlpConfig> mdlpConfig,
        MdlpCheckCisLogService mdlpCheckCisLogService,
        ILogger<MarkingApiService> logger,
        MdlpCashRegHelper mdlpCashRegHelper,
        MarkingOnlineService onlineService,
        MarkingOfflineService offlineService,
        MdlpCheckCisLogRepository mdlpCheckCisLogRepository
        ) : MarkingBaseService(logger)
    {
        /// <summary>
        /// Формат для передачи данных в формате JSON.
        /// </summary>
        public const string APPLICATION_JSON = "application/json";

        /// <summary>
        /// Кэшированный список CDN-хостов.
        /// </summary>
        private List<CdnHost> cachedCdnList = [];

        /// <summary>
        /// Время следующего обновления списка CDN.
        /// </summary>
        private DateTime nextCdnUpdate = DateTime.MinValue;

        /// <summary>
        /// Период обновления списка CDN в минутах.
        /// </summary>
        private readonly double cdnListRefreshPeriod = Convert.ToInt32(
                (mdlpConfig.Value.OnlineService.CdnListExpiryMaxInterval -
                mdlpConfig.Value.OnlineService.CdnListExpiryMinInterval).TotalMinutes);

        /// <summary>
        /// Конфигурация модуля.
        /// </summary>
        private readonly MdlpConfig config = mdlpConfig.Value;

        /// <summary>
        /// Счетчик неудачных попыток обращения к CDN-хосту.
        /// </summary>
        private int cdnHostFailedCount = 0;

        private DateTime GetNextRefreshCdnListTime()
        {
            var nextDelta = new Random().NextDouble() * cdnListRefreshPeriod;
            return DateTime.Now.AddMinutes(nextDelta);
        }

        private async Task ValidateCdnList()
        {
            if (DateTime.Now >= nextCdnUpdate || cachedCdnList.Count == 0)
            {
                await FillCachedCdnList();
                await SortCachedCdnListByLatency();
                return;
            }
        }

        private async Task ReactivateBlockedOnlineHosts()
        {
            var needOrder = false;
            var blockedCdnList = cachedCdnList.Where(x =>
                x.BlockedTo != null
                && x.BlockedTo < DateTime.Now)
                .ToList();
            foreach (var cdnHost in blockedCdnList)
            {
                var latency = await onlineService.GetHostResponseTime(cdnHost.Host);
                if (latency == null)
                {
                    BlockCdnHost(cdnHost);
                    continue;
                }
                logger.LogInformation("Блокировка с узла {cdnHost.Host} снята, " +
                    "задержка {latency} мсек.", cdnHost.Host, latency);
                cdnHost.Latency = (int)latency;
                cdnHost.BlockedTo = null;
                needOrder = true;
            }

            if (needOrder)
                await SortCachedCdnListByLatency();
        }

        private string? SetActiveOnlineHost() => onlineService.CdnHost =
            cachedCdnList
            .Where(x => x.BlockedTo == null)
            .OrderBy(x => x.Latency)
            .FirstOrDefault()
            ?.Host;

        private async Task FillCachedCdnList()
        {
            try
            {
                cachedCdnList = await onlineService.GetCdnList();
                nextCdnUpdate = GetNextRefreshCdnListTime();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "FillCachedCdnList error");
                throw;
            }
        }

        private async Task SortCachedCdnListByLatency()
        {
            List<CdnHost> tmpcdnList = [];
            foreach (var cdn in cachedCdnList)
            {
                var latency = await onlineService.GetHostResponseTime(cdn.Host);
                if (latency == null)
                    continue;
                tmpcdnList.Add(new CdnHost() { Host = cdn.Host, Latency = (int)latency });
            }
            cachedCdnList = [.. tmpcdnList.OrderBy(x => x.Latency)];
            SetActiveOnlineHost();
        }

        /// <summary>
        /// Проверяет КИЗ онлайн и офлайн, возвращает результат проверки.
        /// </summary>
        /// <param name="code">Код КИЗ.</param>
        /// <param name="fiscalSerialNumber">
        /// Фискальный серийный номер (опционально).
        /// </param>
        /// <returns>Результат проверки КИЗ.</returns>
        public virtual async Task<CheckCisDto?> CheckCis(string code, string? fiscalSerialNumber)
        {
            var fixedCode = mdlpCashRegHelper.AutoCorrectWrongCodepage(code);
            config.FiscalSerialNumber = fiscalSerialNumber ?? config.FiscalSerialNumber;

            mdlpCheckCisLogService.SetBaseProps(cis: fixedCode,
                fiscalSerialNumber: config.FiscalSerialNumber);

            var checkResult = await CheckCisOnline(fixedCode)
                ?? await CheckCisOffline(fixedCode);

            if (checkResult == null)
                return null;

            var checkCisDto = ToCheckCisDto(checkResult!, checkResult.IsOnline);
            var respBody = checkResult.Json();
            if (checkCisDto?.Status == false)
                respBody = $"{checkCisDto.Description} {respBody}";

            mdlpCheckCisLogService.SaveLog(checkResult.Host, checkResult.Duration,
                responseBody: respBody, responseStatus: (int)HttpStatusCode.OK,
                isOnline: checkResult.IsOnline, checkIsOk: checkCisDto?.Status);

            return checkCisDto;
        }

        private async Task<CodeCheckResponse?> CheckCisOnline(string code)
        {
            await ValidateCdnList();
            if (GetActiveCachedCdnList().Count == 0)
                throw new ServiceException("Не найдено ни одного действующего хоста для проверки КМ");
            await ReactivateBlockedOnlineHosts();
            var startTime = DateTime.Now;
            var host = onlineService.CdnHost ?? "-";
            try
            {
                return await onlineService.CheckCode(code);
            }
            catch (HttpRequestException ex)
            {
                logger.LogError(ex, "MarkingApiService.CheckCisOnline {code}", code);
                mdlpCheckCisLogService.SaveLog(host, duration: GetDuration(startTime),
                    responseBody: ex.Message, responseStatus: (int?)ex.StatusCode,
                    isOnline: true, checkIsOk: null);
                ProcessFailedCdnHost();
                return null;
            }
            catch (TimeoutException ex)
            {
                logger.LogError(ex, "MarkingApiService.CheckCisOnline {code}", code);
                mdlpCheckCisLogService.SaveLog(host, config.OnlineService.ReqTimeout,
                    responseBody: "", responseStatus: null, isOnline: true, checkIsOk: null);
                ProcessFailedCdnHost();
                return null;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "MarkingApiService.CheckCisOnline {code}", code);
                mdlpCheckCisLogService.SaveLog(host, duration: -1,
                    responseBody: ex.Message, responseStatus: -1,
                    isOnline: true, checkIsOk: null);
                ProcessFailedCdnHost();
                return null;
            }
        }

        private List<CdnHost> GetActiveCachedCdnList() =>
            [.. cachedCdnList.Where(x => x.BlockedTo == null)];

        private async Task<CodeCheckResponse?> CheckCisOffline(string code)
        {
            var startTime = DateTime.Now;
            var host = config.OfflineService.Host;
            try
            {
                var cis = mdlpCashRegHelper.GetCisFromCode(code);
                startTime = DateTime.Now;
                return await offlineService.CheckCis(cis);
            }
            catch (HttpRequestException ex)
            {
                mdlpCheckCisLogService.SaveLog(
                    host, duration: GetDuration(startTime), responseBody: ex.Message,
                    responseStatus: (int?)ex.StatusCode, isOnline: false, checkIsOk: null);
                return null;
            }
            catch (TimeoutException)
            {
                mdlpCheckCisLogService.SaveLog(
                    host, config.OfflineService.ReqTimeout, responseBody: "",
                    responseStatus: null, isOnline: false, checkIsOk: null);
                return null;
            }
        }

        private static int GetDuration(DateTime startTime) =>
            (DateTime.Now - startTime).TotalMilliseconds.ToInt();

        private CheckCisDto ToCheckCisDto(CodeCheckResponse codeCheckResponse, bool isOnline)
        {
            var codeIsOk = ParseCheckedCode(codeCheckResponse.Codes[0], out var description);
            return new()
            {
                Status = codeIsOk,
                Description = description,
                CheckCisResult = new()
                {
                    Uuid = codeCheckResponse.ReqId,
                    Time = codeCheckResponse.ReqTimestamp.ToString(),
                    IsOnline = isOnline,
                    Inst = codeCheckResponse.Inst
                }
            };
        }

        private bool ProcessFailedCdnHost()
        {
            logger.LogDebug("Begin ProcessFailedCdnHost. cdnHostFailedCount: {CdnHostFailedCount}, " +
                "CdnHostFailedMaxCount: {CdnHostFailedMaxCount}, " +
                "ActiveHost: {OnlineServiceCdnHost}", cdnHostFailedCount,
                config.OnlineService.CdnHostFailedMaxCount, onlineService.CdnHost);
            if (cdnHostFailedCount < config.OnlineService.CdnHostFailedMaxCount)
            {
                cdnHostFailedCount++;
                return false;
            }
            var cdnHost = cachedCdnList
                .Where(x => x.Host == onlineService.CdnHost)
                .FirstOrDefault();

            if (cdnHost != null)
                BlockCdnHost(cdnHost);

            return SetActiveOnlineHost() != null;
        }

        private void BlockCdnHost(CdnHost cdnHost) =>
            cdnHost.BlockedTo = DateTime.Now.Add(config.OnlineService.BlockFailedCdnPeriod);

        internal bool ParseCheckedCode(CodeCheckResponse.CheckedCode checkedCode,
            out string? description)
        {
            MarkGroup? markGroup = null;
            if (checkedCode.GroupIds.Count > 0)
            {
                markGroup = FindMarkGroupByCrptCode(checkedCode.GroupIds[0]);
                if (markGroup == null)
                    logger.LogDebug("Не найдена группа маркировки для кода {CrptMarkCode}",
                         checkedCode.GroupIds[0]);
                else
                    logger.LogDebug("Найдена группа маркировки для кода " +
                        "{CrptMarkCode}. {MarkGroupJson}",
                        checkedCode.GroupIds[0], markGroup.Json());
            }
            List<string> disableReasons = [];
            if (checkedCode.Found == false)
                disableReasons.Add("Код не найден");
            if (checkedCode.Valid == false)
                disableReasons.Add("Структура кода не валидная");
            if (checkedCode.Verified == false)
                disableReasons.Add("Проверка крипто-подписи завершилась с ошибкой");
            if (checkedCode.Realizable == false)
                disableReasons.Add("КИ в статусе, отличном от 'В обороте'");
            if (checkedCode.Utilised == false)
                disableReasons.Add("КИ не нанесён»");
            if (markGroup?.CheckIsOwner == true && checkedCode.IsOwner == false)
                disableReasons.Add("КМ не принадлежит участнику, который направил запрос");
            if (checkedCode.IsBlocked == true)
                disableReasons.Add("Продажа заблокирована");
            if ((checkedCode.ErrorCode ?? 0) != 0)
                disableReasons.Add($"Ошибка проверки КМ. Код ошибки: " +
                    $"{checkedCode.ErrorCode}. {checkedCode.Message}");
            if (checkedCode.Sold == true)
                disableReasons.Add("Товар выведен из оборота или имеет " +
                    "признак множественных продаж");
            if (checkedCode.ExpireDate?.Date < DateTime.Today)
                disableReasons.Add($"У товара истёк срок годности " +
                    $"{checkedCode.ExpireDate?.Date.ToShortDateString()}");
            description = disableReasons.Count > 0 ? string.Join(".\n", disableReasons) : null;
            if (!string.IsNullOrEmpty(description))
                description += '.';
            return disableReasons.Count == 0;
        }

        private MarkGroup? FindMarkGroupByCrptCode(int crptCode) =>
            config.MarkGroups.FirstOrDefault(x => x.CrptCode == crptCode);

        public async Task<CheckCisServiceStatus> GetStatus()
        {
            var offlineServiceFailed = await GetOfflineServiceFailed();
            var onlineServiceFailed = await GetOnlineServiceFailed();
            var serviceStatusCode =
                Convert.ToByte(onlineServiceFailed) +
                Convert.ToByte(offlineServiceFailed) * 2;
            return (CheckCisServiceStatus)serviceStatusCode;
        }

        internal async Task<bool> GetOfflineServiceFailed()
        {
            try
            {
                return (await offlineService.GetStatus())?.Status ==
                    LocalModuleStatus.READY;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "GetOfflineServiceFailed");
                return false;
            }
        }

        internal async Task<bool> GetOnlineServiceFailed()
        {
            try
            {
                await ValidateCdnList();
                return GetActiveCachedCdnList().Count == 0;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "GetOnlineServiceFailed");
                return false;
            }
        }

        public List<MdlpCheckCisLog> GetCheckCisHistory(DateTime minDate, DateTime maxDate,
            string? fiscalSerialNumber)
        {
            var history = mdlpCheckCisLogRepository.GetCheckCisHistory(
                minDate, maxDate, fiscalSerialNumber);
            return history;
        }
    }
}
