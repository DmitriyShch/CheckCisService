using CheckCisService.Models.Enums;

namespace CheckCisService.Dto
{
    /// <summary>
    /// DTO-����� ��� �������� ������� ������� �������� ���.
    /// </summary>
    public record GetStatusResponse
    {
        /// <summary>
        /// ������� ������ ������ ������� �������� ���.
        /// </summary>
        public CheckCisServiceStatus Status { get; init; }
    }
}
