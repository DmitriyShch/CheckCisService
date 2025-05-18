using System.Net;
using System.Net.Http.Headers;
using System.Text;
using CheckCisService;
using CheckCisService.Config;
using CheckCisService.Controllers;
using CheckCisService.Models;
using CheckCisService.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace CheckCisServiceTests
{
    /// <summary>
    /// Набор интеграционных тестов для контроллера CheckCisController.
    /// </summary>
    public class CheckCisTests(WebApplicationFactory<Program> factory) :
        IClassFixture<WebApplicationFactory<Program>>
    {
        /// <summary>
        /// HTTP-клиент для отправки запросов к тестируемому сервису.
        /// </summary>
        private readonly HttpClient _client = factory.CreateClient();
        private const string ServiceName = "CheckCis";
        private const string Creds = "admin:1234";

        /// <summary>
        /// Добавляет заголовок авторизации к HTTP-клиенту.
        /// </summary>
        private static void AddAuthorization(HttpClient client, string creds = Creds) =>
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic",
                Convert.ToBase64String(Encoding.UTF8.GetBytes(creds)));

        /// <summary>
        /// Создаёт тестовую конфигурацию для сервиса.
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
        /// Проверяет, что метод GetStatus возвращает 200 и статус Ok.
        /// </summary>
        [Fact]
        public async Task GetStatus_Returns200AndOk()
        {
            // Arrange
            var markingApiServiceMock = new Mock<MarkingApiService>(
                CreateFakeConfig(), null!, null!, null!, null!, null!);

            //markingApiServiceMock
            //    .Setup(s => s.CheckCis(expectedCis, expectedFiscalSerialNumber))
            //    .ReturnsAsync(expectedResult);

            // Здесь можно настроить нужные методы mock, если требуется для теста
            AddAuthorization(_client);

            // Используем TestServer для внедрения mock-сервиса в DI
            var factoryWithMock = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        // Удаляем оригинальный сервис
                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(MarkingApiService));
                        if (descriptor != null)
                            services.Remove(descriptor);
                        // Добавляем mock
                        services.AddSingleton(markingApiServiceMock.Object);
                    });
                });

            var client = factoryWithMock.CreateClient();

            // Act
            var response = await client.GetAsync($"{ServiceName}/Status");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        /// <summary>
        /// Проверяет, что без авторизации возвращается 401.
        /// </summary>
        [Fact]
        public async Task GetStatus_NoAuth_Returns401()
        {
            // Act
            var response = await _client.GetAsync($"{ServiceName}/Status");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }


        /// <summary>
        /// Проверяет, что при неверной авторизации возвращается 401.
        /// </summary>
        [Fact]
        public async Task GetStatus_WrongAuth_Returns401()
        {
            AddAuthorization(_client, "user:100");

            // Act
            var response = await _client.GetAsync($"{ServiceName}/Status");

            // Assert
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        /// <summary>
        /// Проверяет, что метод CheckCis возвращает OkObjectResult с ожидаемым результатом.
        /// </summary>
        [Fact]
        public async Task CheckCis_ReturnsOkObjectResult_WithExpectedResult()
        {
            // Arrange
            var config = CreateFakeConfig();
            var loggerMock = new Mock<ILogger<CheckCisController>>();
            var markingApiServiceMock = new Mock<MarkingApiService>(config, null!, null!, null!, null!, null!);
            var expectedCis = "1234567890";
            var expectedFiscalSerialNumber = "FSN123";
            var expectedResult = new CheckCisDto { CheckCisResult = null };

            markingApiServiceMock
                .Setup(s => s.CheckCis(expectedCis, expectedFiscalSerialNumber))
                .ReturnsAsync(expectedResult);

            var controller = new CheckCisController(loggerMock.Object, markingApiServiceMock.Object);

            // Act
            var result = await controller.CheckCis(expectedCis, expectedFiscalSerialNumber);

            // Assert
            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedResult, okResult.Value);
            markingApiServiceMock.Verify(s => s.CheckCis(expectedCis, expectedFiscalSerialNumber), Times.Once);
        }
    }
}
