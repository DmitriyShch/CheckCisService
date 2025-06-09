using System.Text.Json.Serialization;

namespace CheckCisService.Models
{
    /// <summary>
    /// DTO для передачи результата проверки КИЗ.
    /// </summary>
    public class CheckCisDto
    {
        /// <summary>
        /// Итоговый статус проверки КИЗ (true — успешно, false — ошибка).
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? Status { get; set; }
    
        /// <summary>
        /// Описание результата проверки КИЗ.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? Description { get; init; }

        /// <summary>
        /// Признак превышения таймаута при проверке КИЗ.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public bool? OverTimeout { get; init; } = null;

        /// <summary>
        /// Детализированный результат проверки КИЗ.
        /// </summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public CheckCisResult? CheckCisResult { get; init; }
    }
}
