using CheckCisService.Config;
using CheckCisService.Exceptions;
using CheckCisService.Models;
using CheckCisService.Repositories;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CheckCisServiceTests
{
    /// <summary>
    /// Модульные тесты для репозитория логов проверки КИЗ.
    /// </summary>
    public class MdlpCheckCisLogRepositoryTests
    {
        /// <summary>
        /// Создаёт тестовую конфигурацию для репозитория.
        /// </summary>
        private static IOptions<MdlpConfig> CreateFakeConfig() =>
            Options.Create(new MdlpConfig
            {
                ApiKey = "",
                FiscalSerialNumber = "",
                OfflineService = new OfflineCheckConfig
                {
                    Host = "test-host",
                    Login = "test-login",
                    Pwd = "test-pwd",
                    UrlPrefix = "test-url"
                },
                OnlineService = new OnlineCheckConfig
                {
                    Host = "test-host",
                    UrlPrefix = "test-url"
                },
                DataSource = new DataSourceConfig
                {
                    DataFilePath = ":memory:" // Используем базу данных в памяти
                }
            });

        /// <summary>
        /// Создаёт тестовый логгер для тестов.
        /// </summary>
        public static ILogger<T> CreateLogger<T>() => LoggerFactory
            .Create(builder => builder.AddConsole())
            .CreateLogger<T>();

        /// <summary>
        /// Создаёт экземпляр репозитория для тестов.
        /// </summary>
        private static MdlpCheckCisLogRepository CreateRepository()
        {
            var config = CreateFakeConfig();
            var logger = CreateLogger<MdlpCheckCisLogRepository>();
            return new MdlpCheckCisLogRepository(config, logger);
        }

        /// <summary>
        /// Проверяет, что добавление валидного лога возвращает этот лог.
        /// </summary>
        [Fact]
        public void Add_ValidItem_ReturnsItem()
        {
            // Arrange
            var repository = CreateRepository();
            var logItem = new MdlpCheckCisLog
            {
                SMDocumentId = 1,
                StockPartyId = 2,
                Cis = "test-cis",
                CdnHost = "test-host",
                Duration = 100,
                ResponseBody = "test-response",
                RequestDateTime = DateTime.Now,
                IsOnline = true
            };

            // Act
            var result = repository.Add(logItem);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(logItem.Cis, result.Cis);
            Assert.Equal(logItem.CdnHost, result.CdnHost);
        }

        /// <summary>
        /// Проверяет, что добавление null выбрасывает ServiceException.
        /// </summary>
        [Fact]
        public void Add_NullItem_ThrowsServiceException()
        {
            // Arrange
            var repository = CreateRepository();

            // Act & Assert
            Assert.Throws<ServiceException>(() => repository.Add(null!));
        }
    }
}
