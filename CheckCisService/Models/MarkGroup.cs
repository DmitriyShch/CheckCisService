using CheckCisService.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace CheckCisService.Models;

/// <summary>
/// Описание товарной группы для проверки КИЗ.
/// </summary>
public record MarkGroup
{
    /// <summary>
    /// Требуется ли проверка владельца КМ.
    /// </summary>
    public bool CheckIsOwner { get; set; }

    /// <summary>
    /// Код группы в системе Честный ЗНАК (CRPT).
    /// </summary>
    public int? CrptCode { get; set; }

    /// <summary>
    /// Наименование группы.
    /// </summary>
    public required string Name { get; set; }

    /// <summary>
    /// Дата начала "легкой" проверки.
    /// </summary>
    public DateTime? LightCheckStartDate { get; set; }

    /// <summary>
    /// Дата начала "строгой" проверки.
    /// </summary>
    public DateTime? StrongCheckStartDate { get; set; }
}
