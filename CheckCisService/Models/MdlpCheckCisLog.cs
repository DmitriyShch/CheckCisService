using System.ComponentModel.DataAnnotations;

namespace CheckCisService.Models;

/// <summary>
/// Лог операции проверки КИЗ.
/// </summary>
public class MdlpCheckCisLog: IEntity
{
    /// <summary>
    /// Уникальный идентификатор лога.
    /// </summary>
    [Key]
    public int Id { get; init; }

    /// <summary>
    /// Идентификатор документа.
    /// </summary>
    public int SMDocumentId { get; set; }

    /// <summary>
    /// Идентификатор партии.
    /// </summary>
    public int StockPartyId { get; set; }

    /// <summary>
    /// Код КИЗ.
    /// </summary>
    public required string Cis { get; set; }

    /// <summary>
    /// Идентификатор SGTIN.
    /// </summary>
    public int? SgtinId { get; set; }

    /// <summary>
    /// Фискальный серийный номер.
    /// </summary>
    public string? FiscalSerialNumber { get; set; }

    /// <summary>
    /// Адрес CDN-хоста, использованного для проверки.
    /// </summary>
    public required string CdnHost { get; set; }

    /// <summary>
    /// Длительность выполнения запроса (мс).
    /// </summary>
    public int Duration { get; set; }

    /// <summary>
    /// Тело ответа сервиса.
    /// </summary>
    public required string ResponseBody { get; set; }

    /// <summary>
    /// HTTP-статус ответа.
    /// </summary>
    public int? ResponseStatus { get; set; }

    /// <summary>
    /// Дата и время запроса.
    /// </summary>
    public required DateTime RequestDateTime { get; set; }

    /// <summary>
    /// Признак, что проверка была выполнена онлайн.
    /// </summary>
    public bool IsOnline { get; set; }

    /// <summary>
    /// Итоговый статус проверки КИЗ.
    /// </summary>
    public bool? CheckIsOk { get; set; }
}
