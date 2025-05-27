using CheckCisService.Models.Enums;

namespace CheckCisService.Dto
{
    /// <summary>
    /// DTO-ответ для передачи статуса сервиса проверки КИЗ.
    /// </summary>
    public record GetStatusResponse
    {
        /// <summary>
        /// Текущий статус работы сервиса проверки КИЗ.
        /// </summary>
        public CheckCisServiceStatus Status { get; init; }
    }
}
