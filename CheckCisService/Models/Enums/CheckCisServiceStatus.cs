namespace CheckCisService.Models.Enums
{
    public enum CheckCisServiceStatus
    {
        /// <summary>
        /// Проверка прошла успешно.
        /// </summary>
        Ok = 0,
        /// <summary>
        /// Ошибка онлайн-проверки.
        /// </summary>
        OnlineCheckFailed = 1,
        /// <summary>
        /// Ошибка офлайн-проверки.
        /// </summary>
        OfflineCheckFailed = 2,
        /// <summary>
        /// Ошибка обеих проверок.
        /// </summary>
        AllChecksFailed = 3,
    }
}
