using CheckCisService.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace CheckCisService.Models;

/// <summary>
/// �������� �������� ������ ��� �������� ���.
/// </summary>
public record MarkGroup
{
    /// <summary>
    /// ��������� �� �������� ��������� ��.
    /// </summary>
    public bool CheckIsOwner { get; set; }

    /// <summary>
    /// ��� ������ � ������� ������� ���� (CRPT).
    /// </summary>
    public int? CrptCode { get; set; }

    /// <summary>
    /// ������������ ������.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// ���� ������ "������" ��������.
    /// </summary>
    public DateTime? LightCheckStartDate { get; set; }

    /// <summary>
    /// ���� ������ "�������" ��������.
    /// </summary>
    public DateTime? StrongCheckStartDate { get; set; }
}
