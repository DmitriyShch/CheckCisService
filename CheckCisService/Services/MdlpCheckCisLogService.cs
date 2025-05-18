using CheckCisService.Exceptions;
using CheckCisService.Models;
using CheckCisService.Repositories;

namespace CheckCisService.Services
{
    public class MdlpCheckCisLogService(
        ILogger<MdlpCheckCisLogService> logger,
        MdlpCheckCisLogRepository mdlpCheckCisLogRepository)
    {
        public int? SMDocumentId { get; set; }
        public int? StockPartyId { get; set; }
        public string? Cis { get; set; }
        public int? SgtinId { get; set; }
        public string? FiscalSerialNumber { get; set; }

        public void SetBaseProps(string cis, int smDocumentId = 0, int stockPartyId = 0, 
            int? sgtinId = null, string? fiscalSerialNumber = null)
        {
            SMDocumentId = smDocumentId;
            StockPartyId = stockPartyId;
            Cis = cis;
            SgtinId = sgtinId;
            FiscalSerialNumber = fiscalSerialNumber;
        }

        public MdlpCheckCisLog SaveLog(string? cdnHost, int? duration,
            string responseBody, int? responseStatus, bool isOnline, bool? checkIsOk)
        {
            if (SMDocumentId == null || StockPartyId == null || Cis == null)
            {
                logger.LogError("Не заданы атрибуты документа для логированя. " +
                    "SMDocumentId: {SMDocumentId}, StockPartyId: {StockPartyId}, " +
                    "Cis: {Cis}", SMDocumentId, StockPartyId, Cis);
                throw new ServiceException("Не заданы атрибуты документа для логированя");
            }
            MdlpCheckCisLog mdlpCheckCisLog = new()
            {
                SMDocumentId = (int)SMDocumentId,
                StockPartyId = (int)StockPartyId,
                Cis = Cis,
                SgtinId = SgtinId,
                FiscalSerialNumber = FiscalSerialNumber,
                CdnHost = cdnHost ?? "-",
                Duration = duration ?? -1,
                ResponseBody = responseBody,
                ResponseStatus = responseStatus,
                RequestDateTime = DateTime.Now,
                IsOnline = isOnline,
                CheckIsOk = checkIsOk
            };
            return mdlpCheckCisLogRepository.Add(mdlpCheckCisLog);
        }
    }
}
