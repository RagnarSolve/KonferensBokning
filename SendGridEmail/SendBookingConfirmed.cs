using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

public class SendBookingConfirmed
{
    private readonly IConfiguration _cfg;
    private readonly ILogger<SendBookingConfirmed> _log;

    public SendBookingConfirmed(IConfiguration cfg, ILogger<SendBookingConfirmed> log)
    {
        _cfg = cfg; _log = log;
    }

    public record ConfirmedDto(string to, string name, string bookingId);

    [Function("SendBookingConfirmed")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "email/confirmed")] HttpRequestData req)
    {
        var dto = await JsonSerializer.DeserializeAsync<ConfirmedDto>(req.Body);
        if (dto is null || string.IsNullOrWhiteSpace(dto.to))
            return req.CreateResponse(HttpStatusCode.BadRequest);

        var subject = $"Booking confirmed (#{dto.bookingId})";
        var plain = $"Hi {dto.name},\n\nYour booking #{dto.bookingId} is confirmed.\n\n— Konferenscentrum Väst";
        var html = $"<p>Hi {WebUtility.HtmlEncode(dto.name)},</p>" +
                      $"<p>Your booking <b>#{WebUtility.HtmlEncode(dto.bookingId)}</b> is confirmed.</p>" +
                      $"<p>— Konferenscentrum Väst</p>";

        var ok = await SendAsync(dto.to, dto.name, subject, plain, html);
        return req.CreateResponse(ok ? HttpStatusCode.OK : HttpStatusCode.PreconditionFailed);
    }

    private async Task<bool> SendAsync(string to, string name, string subject, string plain, string html)
    {
        var apiKey = _cfg["SendGrid:ApiKey"];
        var fromEmail = _cfg["Email:FromEmail"];
        var fromName = _cfg["Email:FromName"] ?? "Konferenscentrum Väst";

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            _log.LogWarning("Missing SendGrid:ApiKey (KV ref not resolved?)");
            return false;
        }
        if (string.IsNullOrWhiteSpace(fromEmail))
        {
            _log.LogWarning("Missing Email:FromEmail (KV ref not resolved or not set)");
            return false;
        }

        var client = new SendGridClient(apiKey);
        var msg = new SendGridMessage
        {
            From = new EmailAddress(fromEmail, fromName),
            Subject = subject,
            PlainTextContent = plain,
            HtmlContent = html
        };
        msg.AddTo(new EmailAddress(to, name));

        var resp = await client.SendEmailAsync(msg);
        _log.LogInformation("SendGrid status to {To}: {Status}", to, resp.StatusCode);
        return (int)resp.StatusCode < 400;
    }
}