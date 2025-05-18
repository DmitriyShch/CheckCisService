using CheckCisService.Config;
using CheckCisService.Exceptions;
using CheckCisService.Helpers;
using CheckCisService.Models;
using LiteDB;
using Microsoft.Extensions.Options;

namespace CheckCisService.Repositories;

/// <summary>
/// Репозиторий для работы с логами проверки КИЗ (LiteDB).
/// </summary>
/// <remarks>
/// Создаёт экземпляр репозитория для логов проверки КИЗ.
/// </remarks>
public class MdlpCheckCisLogRepository(
    IOptions<MdlpConfig> mdlpConfig,
    ILogger<MdlpCheckCisLogRepository> logger)
{
    private readonly IOptions<MdlpConfig> mdlpConfig = mdlpConfig;
    private readonly ILogger<MdlpCheckCisLogRepository> logger = logger;

    /// <summary>
    /// Добавляет новый лог проверки КИЗ в базу данных.
    /// </summary>
    /// <param name="item">Лог проверки КИЗ.</param>
    /// <returns>Добавленный лог.</returns>
    public MdlpCheckCisLog Add(MdlpCheckCisLog item)
    {
        using var db = OpenDb();
        try
        {
            var col = db.GetCollection<MdlpCheckCisLog>("MdlpCheckCisLogs");
            var key = col.Insert(item);
            if (key == null)
            {
                logger.LogDebug("Ошибка добавления лога в базу. key == null " +
                    "MdlpCheckCisLog {item}", item.Json());
                throw new ServiceException("Ошибка добавления лога в базу");
            }
            logger.LogDebug("Добавлен лог в базу " +
                "MdlpCheckCisLog {item}", item.Json());
            return item;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при добавлении нового MdlpCheckCisLog {item}",
                item.Json());
            throw new ServiceException("Ошибка при добавлении нового MdlpCheckCisLog");
        }
    }

    /// <summary>
    /// Находит все логи по фискальному номеру и периоду дат.
    /// </summary>
    /// <param name="fiscalSerialNumber">Фискальный серийный номер.</param>
    /// <param name="minDate">Минимальная дата.</param>
    /// <param name="maxDate">Максимальная дата.</param>
    /// <returns>Список логов.</returns>
    public List<MdlpCheckCisLog> FindAllByFiscalSerialNumberAndDatePeriod(
        string fiscalSerialNumber, DateTime minDate, DateTime maxDate)
    {
        using var db = OpenDb();
        try
        {
            var col = db.GetCollection<MdlpCheckCisLog>("MdlpCheckCisLogs");
            var itemList = col
                .Find(x =>
                x.FiscalSerialNumber == fiscalSerialNumber &&
                x.RequestDateTime >= minDate &&
                x.RequestDateTime >= maxDate)
                .OrderBy(x => x.RequestDateTime)
                .ToList();
            logger.LogDebug("Найдено {rowCount} строк логов по " +
                "fiscalSerialNumber: {fiscalSerialNumber}, minDate: {minDate}, " +
                "maxDate: {maxDate}.", itemList.Count, fiscalSerialNumber, minDate, maxDate);
            return itemList;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка при поиске логов. " +
                "fiscalSerialNumber: {fiscalSerialNumber}, minDate: {minDate}, " +
                "maxDate: {maxDate}.", fiscalSerialNumber, minDate, maxDate);
            throw new ServiceException($"Ошибка при поиске логов", ex);
        }
    }

    /// <summary>
    /// Открывает подключение к базе данных LiteDB.
    /// </summary>
    /// <returns>Экземпляр базы данных.</returns>
    private LiteDatabase OpenDb()
    {
        var dataFilePath = mdlpConfig.Value.DataSource.DataFilePath;
        try
        {
            var db = new LiteDatabase(dataFilePath);
            return db;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Ошибка открытия файла базы данных {DataFilePath}", dataFilePath);
            throw new ServiceException($"Ошибка открытия файла базы данных {dataFilePath}", ex);
        }
    }
}