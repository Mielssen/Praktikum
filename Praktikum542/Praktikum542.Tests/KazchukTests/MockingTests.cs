using Xunit;
using FluentAssertions;
using Moq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Hosting;
using Praktikum542.Services;

namespace Praktikum542.Tests.KazchukTests
{
    public class MockingTests
    {
        [Fact]
        public void Configuration_GetSection_ReturnsConfiguredValue()
        {
            var configMock = new Mock<IConfiguration>();
            var sectionMock = new Mock<IConfigurationSection>();

            sectionMock.Setup(s => s.Value).Returns("SuperSecretKey1234567890");
            configMock.Setup(c => c.GetSection("Jwt:Key")).Returns(sectionMock.Object);

            var result = configMock.Object.GetSection("Jwt:Key").Value;

            result.Should().Be("SuperSecretKey1234567890");
            configMock.Verify(c => c.GetSection("Jwt:Key"), Times.Once);
        }

        [Fact]
        public void Configuration_Indexer_ReturnsMockedConnectionString()
        {
            var configMock = new Mock<IConfiguration>();
            configMock.Setup(c => c["ConnectionStrings:DefaultConnection"])
                      .Returns("Server=localhost;Database=TravelManager;");

            var connStr = configMock.Object["ConnectionStrings:DefaultConnection"];

            connStr.Should().Contain("Database=TravelManager");
            configMock.Verify(c => c["ConnectionStrings:DefaultConnection"], Times.Once);
        }

        [Fact]
        public void Logger_LogInformation_MockVerification()
        {
            var loggerMock = new Mock<ILogger<BookingService>>();

            loggerMock.Object.LogInformation("Test log message");

            loggerMock.Invocations.Should().NotBeEmpty();
        }

        [Fact]
        public void WebHostEnvironment_EnvironmentName_ReturnsDevelopment()
        {
            var envMock = new Mock<IWebHostEnvironment>();
            envMock.Setup(e => e.EnvironmentName).Returns("Development");

            var env = envMock.Object.EnvironmentName;

            env.Should().Be("Development");
            envMock.Verify(e => e.EnvironmentName, Times.Once);
        }

        [Fact]
        public void WebHostEnvironment_ContentRootPath_ReturnsMockedPath()
        {
            var envMock = new Mock<IWebHostEnvironment>();
            envMock.Setup(e => e.ContentRootPath).Returns(@"C:\Praktikum542");

            var path = envMock.Object.ContentRootPath;

            path.Should().Be(@"C:\Praktikum542");
            envMock.Verify(e => e.ContentRootPath, Times.Once);
        }
    }
}