namespace CheckCisService.Models
{
    /// <summary>
    /// Ответ сервиса проверки КИЗ, содержащий результат и список проверенных кодов.
    /// </summary>
    public class CodeCheckResponse
    {
        /// <summary>
        /// Результат обработки операции.
        /// </summary>
        public int Code { get; set; }

        /// <summary>
        /// Список проверенных кодов маркировки (КМ).
        /// </summary>
        public List<CheckedCode> Codes { get; set; } = [];

        /// <summary>
        /// Описание результата.
        /// </summary>
        public required string Description { get; set; }

        /// <summary>
        /// Идентификатор экземпляра ПО.
        /// </summary>
        public string? Inst { get; set; }

        /// <summary>
        /// Уникальный идентификатор запроса.
        /// </summary>
        public required string ReqId { get; set; }

        /// <summary>
        /// Время регистрации запроса (UnixTime в мс).
        /// </summary>
        public long ReqTimestamp { get; set; }

        /// <summary>
        /// Адрес хоста, выполнившего проверку.
        /// </summary>
        public string? Host { get; set; }

        /// <summary>
        /// Длительность выполнения запроса (мс).
        /// </summary>
        public int? Duration { get; set; }

        /// <summary>
        /// Признак, что проверка была выполнена онлайн.
        /// </summary>
        public bool IsOnline { get; set; }

        /// <summary>
        /// Информация о проверенном коде маркировки.
        /// </summary>
        public class CheckedCode
        {
            /// <summary>
            /// Код маркировки (КМ).
            /// </summary>
            public required string Cis { get; set; }
            /// <summary>
            /// Признак наличия кода.
            /// </summary>
            public bool? Found { get; set; }
            /// <summary>
            /// Валидность структуры КМ.
            /// </summary>
            public bool? Valid { get; set; }
            /// <summary>
            /// КМ без крипто-подписи.
            /// </summary>
            public string? PrintView { get; set; }
            /// <summary>
            /// Код товара (GTIN).
            /// </summary>
            public string? Gtin { get; set; }
            /// <summary>
            /// Идентификаторы товарных групп.
            /// </summary>
            public List<int> GroupIds { get; set; } = [];
            /// <summary>
            /// Признак успешной проверки крипто-подписи.
            /// </summary>
            public bool? Verified { get; set; }
            /// <summary>
            /// Признак ввода в оборот.
            /// </summary>
            public bool? Realizable { get; set; }
            /// <summary>
            /// Признак нанесения КИ на упаковку.
            /// </summary>
            public bool? Utilised { get; set; }
            /// <summary>
            /// Срок годности.
            /// </summary>
            public DateTime? ExpireDate { get; set; }
            /// <summary>
            /// Дата производства.
            /// </summary>
            public DateTime? ProductionDate { get; set; }
            /// <summary>
            /// Признак принадлежности кода владельцу.
            /// </summary>
            public bool? IsOwner { get; set; }
            /// <summary>
            /// Признак блокировки продажи.
            /// </summary>
            public bool? IsBlocked { get; set; }
            /// <summary>
            /// Органы госвласти, установившие блокировку.
            /// </summary>
            public List<string> Ogvs { get; set; } = [];
            /// <summary>
            /// Сообщение об ошибке.
            /// </summary>
            public string? Message { get; set; }
            /// <summary>
            /// Код ошибки.
            /// </summary>
            public int? ErrorCode { get; set; }
            /// <summary>
            /// Признак прослеживаемости.
            /// </summary>
            public bool? IsTracking { get; set; }
            /// <summary>
            /// Признак вывода из оборота.
            /// </summary>
            public bool? Sold { get; set; }
            /// <summary>
            /// Состояние выбытия.
            /// </summary>
            public int? EliminationState { get; set; }
            /// <summary>
            /// Максимальная розничная цена.
            /// </summary>
            public decimal? Mrp { get; set; }
            /// <summary>
            /// Минимальная единая цена.
            /// </summary>
            public decimal? Smp { get; set; }
            /// <summary>
            /// Признак «серой зоны».
            /// </summary>
            public bool? GrayZone { get; set; }
            /// <summary>
            /// Количество в упаковке.
            /// </summary>
            public int? InnerUnitCount { get; set; }
            /// <summary>
            /// Проданное количество.
            /// </summary>
            public int? SoldUnitCount { get; set; }
            /// <summary>
            /// Тип упаковки.
            /// </summary>
            public string? PackageType { get; set; }
            /// <summary>
            /// ИНН производителя.
            /// </summary>
            public string? ProducerInn { get; set; }
        }
    }
}
