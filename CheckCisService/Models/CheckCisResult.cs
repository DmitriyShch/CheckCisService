using System.Text.Json.Serialization;

namespace CheckCisService.Models;

/// <summary>
/// Детализированный результат проверки КИЗ.
/// </summary>
public class CheckCisResult()
{
    /// <summary>
    /// Уникальный идентификатор запроса.
    /// </summary>
    public required string Uuid { get; init; }

    /// <summary>
    /// Время запроса (UnixTime в мс).
    /// </summary>
    public required long Time { get; init; }
    
    /// <summary>
    /// Идентификатор экземпляра ПО (опционально).
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Inst { get; init; }
    
    /// <summary>
    /// Версия ПО (опционально).
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Ver { get; init; }

    /// <summary>
    /// Признак, что проверка была выполнена онлайн.
    /// </summary>
    public bool IsOnline { get; init; }
}
