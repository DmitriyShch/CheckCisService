using CheckCisService.Config;
using CheckCisService.Exceptions;
using CheckCisService.Models;
using CheckCisService.Services;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Reflection;

namespace CheckCisServiceTests
{
    /// <summary>
    /// Модульные тесты для класса MarkingOfflineService.
    /// </summary>
    public class MarkingOfflineServiceUnitTests
    {
        /// <summary>
        /// Создаёт тестовую конфигурацию для офлайн-сервиса.
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
        /// Проверяет корректность формирования StatusUri для разных вариантов входных данных.
        /// </summary>
        [Fact]
        public void CheckCisUriResolver_GetStatusUri()
        {
            GetStatusUri(baseHost: "http://127.0.0.1:5995/", apiPrefix: "api/v2/",
                expectedStatusUri: "http://127.0.0.1:5995/api/v2/status");

            GetStatusUri(baseHost: "http://127.0.0.1:5995", apiPrefix: "api/v2/",
                        expectedStatusUri: "http://127.0.0.1:5995/api/v2/status");

            GetStatusUri(baseHost: "http://127.0.0.1:5995", apiPrefix: "api/v2",
                        expectedStatusUri: "http://127.0.0.1:5995/api/v2/status");

            GetStatusUri(baseHost: "http://127.0.0.1:5995/", apiPrefix: "/api/v2/",
                    expectedStatusUri: "http://127.0.0.1:5995/api/v2/status");
        }

        /// <summary>
        /// Проверяет, что StatusUri формируется корректно.
        /// </summary>
        private static void GetStatusUri(string baseHost, string apiPrefix,
            string expectedStatusUri)
        {
            var cisUriResolver = new MarkingOfflineService.CheckCisUriResolver(baseHost, apiPrefix);
            var statusUri = cisUriResolver.StatusUri.ToString();
            Assert.Equal(expectedStatusUri, statusUri);
        }

        /// <summary>
        /// Создаёт экземпляр MarkingOfflineService для тестов.
        /// </summary>
        private static MarkingOfflineService MakeOfflineService()
        {
            var logger = MdlpCheckCisLogRepositoryTests.CreateLogger<MarkingOfflineService>();
            var config = CreateFakeConfig();
            var service = new MarkingOfflineService(logger, config);
            return service;
        }

        /// <summary>
        /// Проверяет успешный ответ метода GetStatus.
        /// </summary>
        [Fact]
        public async Task GetStatus_SuccessfulResponse_ReturnsDeserializedDto()
        {
            // Arrange  
            var expectedDto = new MarkingModuleStatusDto()
            {
                Status = LocalModuleStatus.READY,
                OperationMode = "active",
                Version = "1.0",
                Inst = "test-instance",
                Name = "test-name"
            };
            var service = MakeOfflineService();

            // Act  
            var result = await service.GetStatus();

            // Assert  
            Assert.Equal(expectedDto.Status, result!.Status);
            Assert.Equal(expectedDto.OperationMode, result.OperationMode);
            Assert.True(Convert.ToInt32(result.ReplicationStatus?.BlockedSeries?.LocalDocCount) > 0);
        }

        /// <summary>
        /// Проверяет, что при ошибке HTTP выбрасывается ServiceException.
        /// </summary>
        [Fact]
        public async Task GetStatus_HttpError_ThrowsIssException()
        {
            // Arrange
            var httpMessageHandler = new Mock<HttpMessageHandler>();
            httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            var httpClient = new HttpClient(httpMessageHandler.Object);
            var service = MakeOfflineService();
            
            // Use reflection to set the private HttpClient field  
            var httpClientField = typeof(MarkingOfflineService)
                .GetField("checkCodeHttpClient", BindingFlags.NonPublic | BindingFlags.Instance);
            httpClientField?.SetValue(service, httpClient);

            // Act & Assert
            await Assert.ThrowsAsync<ServiceException>(service.GetStatus);
        }

        /// <summary>
        /// Проверяет, что при таймауте выбрасывается TimeoutException.
        /// </summary>
        [Fact]
        public async Task CheckCis_Timeout_ReturnsFailedDto()
        {
            // Arrange
            var code = "test-code";
            var httpMessageHandler = new Mock<HttpMessageHandler>();
            httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new TaskCanceledException());

            var httpClient = new HttpClient(httpMessageHandler.Object);
            var service = MakeOfflineService();

            // Use reflection to set the private HttpClient field  
            var httpClientField = typeof(MarkingOfflineService)
                .GetField("checkCodeHttpClient", BindingFlags.NonPublic | BindingFlags.Instance);
            httpClientField?.SetValue(service, httpClient);

            // Act & Assert
            await Assert.ThrowsAsync<TimeoutException>(() => { return service.CheckCis(code); });
        }

        /// <summary>
        /// Проверяет, что при отсутствии ответа выбрасывается HttpRequestException.
        /// </summary>
        [Fact]
        public async Task CheckCis_NullResponse_ReturnsFailedDto()
        {
            // Arrange
            var code = "test-code";

            var httpMessageHandler = new Mock<HttpMessageHandler>();
            httpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage?>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpResponseMessage?)null);

            var httpClient = new HttpClient(httpMessageHandler.Object);
            var service = MakeOfflineService();

            // Use reflection to set the private HttpClient field  
            var httpClientField = typeof(MarkingOfflineService)
                .GetField("checkCodeHttpClient", BindingFlags.NonPublic | BindingFlags.Instance);
            httpClientField?.SetValue(service, httpClient);

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => { return service.CheckCis(code); });
        }
    }
}
