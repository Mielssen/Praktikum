using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Praktikum542.Models;
using Praktikum542.Services;

namespace Praktikum542.Tests.ToderikoTests.Helpers;

/// <summary>
/// Піднімає застосунок у пам'яті з:
///  - власною in-memory БД (унікальна назва, не перетинається з тестами колег)
///  - FakeEmailService замість реального SMTP (щоб тести не слали справжні листи)
/// </summary>
public class PasswordResetWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = "ToderikoDb_" + Guid.NewGuid();

    public FakeEmailService Emails { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
            var dbDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<PraktikumContext>));

            if (dbDescriptor != null)
                services.Remove(dbDescriptor);

            services.AddDbContext<PraktikumContext>(options =>
                options.UseInMemoryDatabase(_databaseName));

            var emailDescriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(IEmailService));

            if (emailDescriptor != null)
                services.Remove(emailDescriptor);

            services.AddSingleton<IEmailService>(Emails);
        });
    }
}