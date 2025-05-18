namespace CheckCisService.Models
{
    /// <summary>
    /// Интерфейс для сущностей с идентификатором.
    /// </summary>
    public interface IEntity
    {
        /// <summary>
        /// Уникальный идентификатор сущности.
        /// </summary>
        int Id { get; init; }
    }
}
