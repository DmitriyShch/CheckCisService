using Newtonsoft.Json;

namespace CheckCisService.Models;

/// <summary>
/// DTO для передачи статуса локального модуля проверки КИЗ.
/// </summary>
public class MarkingModuleStatusDto
{
    /// <summary>
    /// Время последней синхронизации (UnixTime в мс).
    /// </summary>
    public long LastSync { get; set; }

    /// <summary>
    /// Версия программного обеспечения.
    /// </summary>
    public required string Version { get; set; }

    /// <summary>
    /// Идентификатор экземпляра ПО.
    /// </summary>
    public required string Inst { get; set; }

    /// <summary>
    /// Наименование программного обеспечения.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Статус ПО (например, "ready", "sync_error").
    /// </summary>
    public LocalModuleStatus Status { get; set; }

    /// <summary>
    /// Режим работы ("active", "service").
    /// </summary>
    public required string OperationMode { get; set; }

    /// <summary>
    /// Требуется ли загрузка базы данных.
    /// </summary>
    public bool? RequiresDownload { get; set; }

    /// <summary>
    /// Статус репликации данных.
    /// </summary>
    public ReplicationStatusDto? ReplicationStatus { get; set; }
}

/// <summary>
/// DTO для передачи статуса репликации данных.
/// </summary>
public class ReplicationStatusDto
{
    /// <summary>
    /// Данные по КМ.
    /// </summary>
    public ReplicationDataDto? Cis { get; set; }

    /// <summary>
    /// Данные по заблокированным сериям.
    /// </summary>
    [JsonProperty("blocked_series")]
    public ReplicationDataDto? BlockedSeries { get; set; }

    /// <summary>
    /// Данные по заблокированным GTIN.
    /// </summary>
    [JsonProperty("blocked_gtin")]
    public ReplicationDataDto? BlockedGtin { get; set; }

    /// <summary>
    /// Данные по заблокированным КМ.
    /// </summary>
    [JsonProperty("blocked_cis")]
    public ReplicationDataDto? BlockedCis { get; set; }
}

/// <summary>
/// DTO для передачи информации о репликации по объекту.
/// </summary>
public class ReplicationDataDto
{
    /// <summary>
    /// Временная задержка.
    /// </summary>
    public string? TimeLag { get; set; }

    /// <summary>
    /// Количество документов на сервере.
    /// </summary>
    public string? ServerDocCount { get; set; }

    /// <summary>
    /// Количество документов локально.
    /// </summary>
    public string? LocalDocCount { get; set; }
}

/// <summary>
/// Перечисление статусов локального модуля.
/// </summary>
public enum LocalModuleStatus
{
    INITIALIZATION,
    NOT_CONFIGURED,
    READY,
    SYNC_ERROR
}
