using CheckCisService.Config;
using CheckCisService.Exceptions;
using CheckCisService.Helpers;
using CheckCisService.Models;
using LiteDB;
using Microsoft.Extensions.Options;

namespace CheckCisService.Repositories;

/// <summary>
/// ����������� ��� ������ � ������ �������� ��� (LiteDB).
/// </summary>
/// <remarks>
/// ������ ��������� ����������� ��� ����� �������� ���.
/// </remarks>
public class MdlpCheckCisLogRepository(
    IOptions<MdlpConfig> mdlpConfig,
    ILogger<MdlpCheckCisLogRepository> logger)
{
    private readonly IOptions<MdlpConfig> mdlpConfig = mdlpConfig;
    private readonly ILogger<MdlpCheckCisLogRepository> logger = logger;

    /// <summary>
    /// ��������� ����� ��� �������� ��� � ���� ������.
    /// </summary>
    /// <param name="item">��� �������� ���.</param>
    /// <returns>����������� ���.</returns>
    public MdlpCheckCisLog Add(MdlpCheckCisLog item)
    {
        using var db = OpenDb();
        try
        {
            var col = db.GetCollection<MdlpCheckCisLog>("MdlpCheckCisLogs");
            var key = col.Insert(item);
            if (key == null)
            {
                logger.LogDebug("������ ���������� ���� � ����. key == null " +
                    "MdlpCheckCisLog {item}", item.Json());
                throw new ServiceException("������ ���������� ���� � ����");
            }
            logger.LogDebug("�������� ��� � ���� " +
                "MdlpCheckCisLog {item}", item.Json());
            return item;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "������ ��� ���������� ������ MdlpCheckCisLog {item}",
                item.Json());
            throw new ServiceException("������ ��� ���������� ������ MdlpCheckCisLog");
        }
    }

    /// <summary>
    /// ������� ��� ���� �� ����������� ������ � ������� ���.
    /// </summary>
    /// <param name="fiscalSerialNumber">���������� �������� �����.</param>
    /// <param name="minDate">����������� ����.</param>
    /// <param name="maxDate">������������ ����.</param>
    /// <returns>������ �����.</returns>
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
            logger.LogDebug("������� {rowCount} ����� ����� �� " +
                "fiscalSerialNumber: {fiscalSerialNumber}, minDate: {minDate}, " +
                "maxDate: {maxDate}.", itemList.Count, fiscalSerialNumber, minDate, maxDate);
            return itemList;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "������ ��� ������ �����. " +
                "fiscalSerialNumber: {fiscalSerialNumber}, minDate: {minDate}, " +
                "maxDate: {maxDate}.", fiscalSerialNumber, minDate, maxDate);
            throw new ServiceException($"������ ��� ������ �����", ex);
        }
    }

    /// <summary>
    /// ��������� ����������� � ���� ������ LiteDB.
    /// </summary>
    /// <returns>��������� ���� ������.</returns>
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
            logger.LogError(ex, "������ �������� ����� ���� ������ {DataFilePath}", dataFilePath);
            throw new ServiceException($"������ �������� ����� ���� ������ {dataFilePath}", ex);
        }
    }
}