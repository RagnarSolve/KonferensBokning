using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SendGrid;
using SendGrid.Helpers.Mail;

public class SendBookingCancelled
{
    private readonly IConfiguration _cfg;
    private readonly ILogger<SendBookingCancelled> _log;

    public SendBookingCancelled(IConfiguration cfg, ILogger<SendBookingCancelled> log)
    {
        _cfg = cfg; _log = log;
    }

    public record CancelledDto(string to, string name, string bookingId, string? reason);

    [Function("SendBookingCancelled")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "email/cancelled")] HttpRequestData req)
    {
        var dto = await JsonSerializer.DeserializeAsync<CancelledDto>(req.Body);
        if (dto is null || string.IsNullOrWhiteSpace(dto.to))
            return req.CreateResponse(HttpStatusCode.BadRequest);

        var subject   = $"Booking cancelled (#{dto.bookingId})";
        var reasonTxt = string.IsNullOrWhiteSpace(dto.reason) ? "" : $" Reason: {dto.reason}";
        var plain     = $"Hi {dto.name},\n\nYour booking #{dto.bookingId} has been cancelled.{reasonTxt}\n\n— Konferenscentrum Väst";
        var reasonHtml= string.IsNullOrWhiteSpace(dto.reason) ? "" : $"<p><i>Reason:</i> {WebUtility.HtmlEncode(dto.reason)}</p>";
        var html      = $"<p>Hi {WebUtility.HtmlEncode(dto.name)},</p>" +
                        $"<p>Your booking <b>#{WebUtility.HtmlEncode(dto.bookingId)}</b> has been cancelled.</p>" +
                         reasonHtml + "<p>— Konferenscentrum Väst</p>";

        var ok = await SendAsync(dto.to, dto.name, subject, plain, html);
        return req.CreateResponse(ok ? HttpStatusCode.OK : HttpStatusCode.PreconditionFailed);
    }

    private async Task<bool> SendAsync(string to, string name, string subject, string plain, string html)
    {
        var apiKey    = _cfg["SendGrid:ApiKey"];
        var fromEmail = _cfg["Email:FromEmail"];
        var fromName = _cfg["Email:FromName"];

        if (string.IsNullOrWhiteSpace(apiKey))
        { _log.LogWarning("Missing SendGrid:ApiKey"); return false; }

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