using System.Net.Http.Json;
using ApiService.Configuration;
using Microsoft.Extensions.Options;

namespace ApiService.Services.Communications;

public class EmailDeliveryService(HttpClient http, IOptions<EmailOptions> options)
{
    private record BrevoContact(string Email, string? Name = null);

    private record BrevoEmailRequest(
        BrevoContact Sender,
        List<BrevoContact> To,
        string Subject,
        string TextContent,
        BrevoContact? ReplyTo);

    private record BrevoEmailResponse(string? MessageId);

    public async Task SendPlainTextEmailAsync(IEnumerable<string>? recipients, string? subject, string? body)
    {
        var list = recipients?.ToList();
        if (list is null || list.Count == 0)
        {
            throw new ArgumentException("At least one recipient email is required");
        }
        if (string.IsNullOrWhiteSpace(subject))
        {
            throw new ArgumentException("Rendered email subject is empty");
        }
        if (string.IsNullOrWhiteSpace(body))
        {
            throw new ArgumentException("Rendered email body is empty");
        }

        var config = options.Value;
        var apiKey = TrimToNull(config.Brevo.ApiKey)
            ?? throw new InvalidOperationException("Brevo API key is not configured");

        var to = list
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(r => new BrevoContact(r.Trim()))
            .ToList();
        if (to.Count == 0)
        {
            throw new ArgumentException("At least one recipient email is required");
        }

        var replyTo = TrimToNull(config.ReplyTo);
        var message = new BrevoEmailRequest(
            new BrevoContact(config.FromAddress, config.FromName),
            to,
            subject,
            body,
            replyTo is null ? null : new BrevoContact(replyTo));

        using var request = new HttpRequestMessage(HttpMethod.Post, "smtp/email")
        {
            Content = JsonContent.Create(message, options: BrevoJson.Options),
        };
        request.Headers.Add("api-key", apiKey);

        using var response = await http.SendAsync(request);
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException(
                $"Unable to send email [HTTP {(int)response.StatusCode}]: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<BrevoEmailResponse>(BrevoJson.Options);
        if (TrimToNull(result?.MessageId) is null)
        {
            throw new InvalidOperationException("Brevo did not return a messageId");
        }
    }

    private static string? TrimToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
