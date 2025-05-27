using CheckCisService.Models;

namespace CheckCisService.Config
{
    /// <summary>
    /// Конфигурация основного модуля проверки КИЗ.
    /// </summary>
    public class MdlpConfig
    {
        /// <summary>
        /// API-ключ для доступа к сервису проверки КИЗ.
        /// </summary>
        public required string ApiKey { get; init; }

        /// <summary>
        /// Конфигурация источника данных для работы сервиса.
        /// </summary>
        public required DataSourceConfig DataSource { get; init; }

        /// <summary>
        /// Фискальный серийный номер, используемый при запросах.
        /// </summary>
        public required string FiscalSerialNumber { get; set; }

        /// <summary>
        /// Список групп маркировки, поддерживаемых сервисом.
        /// </summary>
        public MarkGroup[] MarkGroups { get; init; } = [];

        /// <summary>
        /// Конфигурация офлайн-сервиса проверки КИЗ.
        /// </summary>
        public required OfflineCheckConfig OfflineService { get; init; }

        /// <summary>
        /// Конфигурация онлайн-сервиса проверки КИЗ.
        /// </summary>
        public required OnlineCheckConfig OnlineService { get; init; }

        public int GetHistoryMaxRecordCount { get; init; } = 100;
    }

    /// <summary>
    /// Конфигурация онлайн-сервиса проверки КИЗ.
    /// </summary>
    public class OnlineCheckConfig
    {
        /// <summary>
        /// Период блокировки CDN-хоста после неудачных попыток (TimeSpan).
        /// </summary>
        public TimeSpan BlockFailedCdnPeriod { get; init; }

        /// <summary>
        /// Таймаут проверки доступности CDN-хоста (в миллисекундах).
        /// </summary>
        public int CheckHealthReqTimeout { get; init; }

        /// <summary>
        /// Максимальное количество неудачных попыток обращения к CDN-хосту.
        /// </summary>
        public int CdnHostFailedMaxCount { get; init; }

        /// <summary>
        /// Максимальный интервал обновления списка CDN-хостов.
        /// </summary>
        public TimeSpan CdnListExpiryMaxInterval { get; init; }

        /// <summary>
        /// Минимальный интервал обновления списка CDN-хостов.
        /// </summary>
        public TimeSpan CdnListExpiryMinInterval { get; init; }

        /// <summary>
        /// Адрес основного CDN-хоста для онлайн-проверки.
        /// </summary>
        public required string Host { get; init; }

        /// <summary>
        /// Таймаут запроса проверки КИЗ (в миллисекундах).
        /// </summary>
        public int ReqTimeout { get; init; }

        /// <summary>
        /// Префикс URL для запросов к онлайн-сервису.
        /// </summary>
        public required string UrlPrefix { get; init; }
    }

    /// <summary>
    /// Конфигурация офлайн-сервиса проверки КИЗ.
    /// </summary>
    public class OfflineCheckConfig
    {
        /// <summary>
        /// Адрес офлайн-сервиса проверки КИЗ.
        /// </summary>
        public required string Host { get; init; }

        /// <summary>
        /// Логин для авторизации в офлайн-сервисе.
        /// </summary>
        public required string Login { get; init; }

        /// <summary>
        /// Пароль для авторизации в офлайн-сервисе.
        /// </summary>
        public required string Pwd { get; init; }

        /// <summary>
        /// Таймаут запроса проверки КИЗ (в миллисекундах).
        /// </summary>
        public int ReqTimeout { get; init; }

        /// <summary>
        /// Префикс URL для запросов к офлайн-сервису.
        /// </summary>
        public required string UrlPrefix { get; init; }
    }

    /// <summary>
    /// Конфигурация источника данных для сервиса.
    /// </summary>
    public class DataSourceConfig
    {
        /// <summary>
        /// Путь к файлу с данными для работы сервиса.
        /// </summary>
        public required string DataFilePath { get; init; }
    }
}
