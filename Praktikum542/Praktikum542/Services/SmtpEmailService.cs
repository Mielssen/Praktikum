
using System.Net;
using System.Net.Mail;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using Polly.Timeout;

namespace Praktikum542.Services
{
    public class SmtpEmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<SmtpEmailService> _logger;
        private readonly ResiliencePipeline _pipeline;

        public SmtpEmailService(
            IConfiguration config,
            ILogger<SmtpEmailService> logger)
        {
            _config = config;
            _logger = logger;

            _pipeline = new ResiliencePipelineBuilder()
                .AddRetry(new RetryStrategyOptions
                {
                    MaxRetryAttempts = 2,
                    Delay = TimeSpan.FromSeconds(1),
                    BackoffType = DelayBackoffType.Exponential,
                    ShouldHandle = new PredicateBuilder()
                        .Handle<SmtpException>()
                        .Handle<TimeoutRejectedException>()
                })
                .AddCircuitBreaker(new CircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.5,
                    MinimumThroughput = 4,
                    SamplingDuration = TimeSpan.FromSeconds(30),
                    BreakDuration = TimeSpan.FromSeconds(20),
                    ShouldHandle = new PredicateBuilder()
                        .Handle<SmtpException>()
                        .Handle<TimeoutRejectedException>()
                })
                .AddTimeout(TimeSpan.FromSeconds(10))
                .Build();
        }

        public async Task SendAsync(
            string to,
            string subject,
            string htmlBody)
        {
            var host = _config["Smtp:Host"]
                ?? throw new InvalidOperationException("Не налаштовано Smtp:Host");

            if (!int.TryParse(_config["Smtp:Port"], out var port))
                throw new InvalidOperationException("Некоректно налаштовано Smtp:Port");

            var user = _config["Smtp:User"];
            var pass = _config["Smtp:Password"];
            var from = _config["Smtp:From"] ?? user;

            if (string.IsNullOrWhiteSpace(from))
                throw new InvalidOperationException("Не налаштовано адресу відправника SMTP");

            try
            {
                await _pipeline.ExecuteAsync(async cancellationToken =>
                {
                    using var client = new SmtpClient(host, port)
                    {
                        Credentials = new NetworkCredential(user, pass),
                        EnableSsl = true
                    };

                    using var message = new MailMessage(
                        from, to, subject, htmlBody)
                    {
                        IsBodyHtml = true
                    };

                    await client.SendMailAsync(message, cancellationToken);
                });

                _logger.LogInformation(
                    "Лист надіслано на {Email}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex, "Не вдалося надіслати лист на {Email}", to);

                throw;
            }
        }
    }
}
