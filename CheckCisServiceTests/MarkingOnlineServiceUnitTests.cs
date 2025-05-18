using CheckCisService.Config;
using CheckCisService.Services;
using Microsoft.Extensions.Options;

namespace CheckCisServiceTests
{
    /// <summary>
    /// Модульные тесты для класса MarkingOnlineService.
    /// </summary>
    public class MarkingOnlineServiceUnitTests
    {
        /// <summary>
        /// Создаёт тестовую конфигурацию для онлайн-сервиса.
        /// </summary>
        private static IOptions<MdlpConfig> CreateFakeConfig() =>
             Options.Create(new MdlpConfig
             {
                 ApiKey = "61ec1bbb-211f-4289-820c-a140cb94c99a",
                 FiscalSerialNumber = "1209",
                 OnlineService = new()
                 {
                     UrlPrefix = "api/v4/true-api/",
                     Host = "https://markirovka.sandbox.crptech.ru/",
                     CheckHealthReqTimeout = 2000,
                     ReqTimeout = 2000,
                 },
                 OfflineService = new()
                 {
                     Host = "http://192.168.1.85:5995/",
                     Login = "admin",
                     UrlPrefix = "/api/v1/",
                     Pwd = "1234",
                     ReqTimeout = 1000
                 },
                 DataSource = new DataSourceConfig
                 {
                     DataFilePath = ":memory:" // Используем базу данных в памяти
                 }
             });

        /// <summary>
        /// Создаёт экземпляр MarkingOnlineService для тестов.
        /// </summary>
        private static MarkingOnlineService MakeOnlineService()
        {
            var logger = MdlpCheckCisLogRepositoryTests.CreateLogger<MarkingOnlineService>();
            var config = CreateFakeConfig();
            var service = new MarkingOnlineService(logger, config);
            return service;
        }

        /// <summary>
        /// Проверяет успешное получение списка CDN-хостов.
        /// </summary>
        [Fact]
        public async Task GetStatus_SuccessfulResponse_ReturnsDeserializedDto()
        {
            // Arrange  
            var service = MakeOnlineService();

            // Act  
            var result = await service.GetCdnList();

            // Assert  
            Assert.True(result?.Count > 0);
        }

        /// <summary>
        /// Проверяет успешное получение времени отклика CDN-хоста.
        /// </summary>
        [Fact]
        public async Task GetHostResponseTime_Successful()
        {
            // Arrange  
            var service = MakeOnlineService();
            var config = CreateFakeConfig();
            var maxResponseTime = config.Value.OnlineService.CheckHealthReqTimeout;

            // Act  
            var result = await service.GetCdnList();
            Assert.True(result?.Count > 0);
            var host = result[0];
            var responseTime = await service.GetHostResponseTime(host.Host);

            // Assert  
            Assert.True(responseTime < maxResponseTime, $"responseTime ({responseTime}) " +
                $"is more than {maxResponseTime} msec");
        }

        /// <summary>
        /// Проверяет успешную проверку кода через онлайн-сервис.
        /// </summary>
        [Fact]
        public async Task CheckCode_Successful()
        {
            // Arrange  
            var service = MakeOnlineService();
            var code = "01048657365749062155esJWe\u001d93dGVz";
            var successfulCode = 0;

            // Act  
            var result = await service.GetCdnList();
            Assert.True(result?.Count > 0);
            var host = result[0];
            service.CdnHost = host.Host;
            var response = await service.CheckCode(code);

            // Assert  
            Assert.True(response.Code == successfulCode, $"response.Code is bad ({response.Code})");
        }
    }
}
