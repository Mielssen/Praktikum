using Praktikum542.Services;

namespace Praktikum542.Tests.ToderikoTests.Helpers;

public record SentEmail(string To, string Subject, string Body);

/// <summary>
/// Підміна IEmailService для інтеграційних/E2E тестів:
/// замість реальної відправки через SMTP просто запам'ятовує листи.
/// </summary>
public class FakeEmailService : IEmailService
{
    private readonly List<SentEmail> _sent = new();

    public IReadOnlyList<SentEmail> Sent
    {
        get { lock (_sent) { return _sent.ToList(); } }
    }

    public IReadOnlyList<SentEmail> SentTo(string to) =>
        Sent.Where(e => e.To == to).ToList();

    public Task SendAsync(string to, string subject, string htmlBody)
    {
        lock (_sent)
        {
            _sent.Add(new SentEmail(to, subject, htmlBody));
        }

        return Task.CompletedTask;
    }
}