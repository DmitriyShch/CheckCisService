namespace CheckCisService.Models
{
    /// <summary>
    /// Ответ сервиса, содержащий список доступных CDN-хостов для проверки КИЗ.
    /// </summary>
    public class CdnListResponse
    {
        /// <summary>
        /// Массив доступных CDN-хостов.
        /// </summary>
        public required CdnHost[] Hosts { get; set; } = [];

        /// <summary>
        /// Описание отдельного CDN-хоста.
        /// </summary>
        public class CdnHost
        {
            /// <summary>
            /// Адрес CDN-хоста.
            /// </summary>
            public required string Host { get; init; }

            /// <summary>
            /// Задержка (latency) при обращении к хосту, в миллисекундах.
            /// </summary>
            public int Latency { get; set; }

            /// <summary>
            /// Дата и время, до которых хост заблокирован (если заблокирован).
            /// </summary>
            public DateTime? BlockedTo { get; set; }
        }
    }
}
