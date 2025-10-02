using System.Net.Http;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace KonferenscentrumVast.Services
{
    public class SendGridService
    {
        private readonly IHttpClientFactory _http;
        private readonly IConfiguration _cfg;
        private readonly ILogger<SendGridService> _log;

        public SendGridService(IHttpClientFactory http, IConfiguration cfg, ILogger<SendGridService> log)
        {
            _http = http; _cfg = cfg; _log = log;
        }

        public Task SendBookingConfirmedAsync(string toEmail, string name, string bookingId) =>
            PostAsync(_cfg["SendGridEmail:OnConfirmed"], new { to = toEmail, name, bookingId });

        public Task SendBookingCancelledAsync(string toEmail, string name, string bookingId, string? reason) =>
            PostAsync(_cfg["SendGridEmail:OnCancelled"], new { to = toEmail, name, bookingId, reason });

        private async Task PostAsync(string? url, object payload)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                _log.LogWarning("Email URL missing.");
                return;
            }

            var json = JsonSerializer.Serialize(payload);
            var resp = await _http.CreateClient().PostAsync(
                url, new StringContent(json, Encoding.UTF8, "application/json"));

            var body = await resp.Content.ReadAsStringAsync();
            _log.LogInformation("SendGridEmail POST {Url} -> {Status} {Body}", url, (int)resp.StatusCode, body);
        }
    }
}