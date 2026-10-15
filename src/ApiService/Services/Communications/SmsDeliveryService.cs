using System.Net.Http.Json;
using System.Text.RegularExpressions;
using ApiService.Configuration;
using Microsoft.Extensions.Options;

namespace ApiService.Services.Communications;

public partial class SmsDeliveryService(
    HttpClient http,
    IOptions<SmsOptions> smsOptions,
    IOptions<EmailOptions> emailOptions)
{
    private static readonly HashSet<string> SupportedTypes = ["transactional", "marketing"];

    private record BrevoSmsRequest(
        string Sender,
        string Recipient,
        string Content,
        string Type,
        string? Tag,
        string? WebUrl,
        bool UnicodeEnabled,
        string? OrganisationPrefix);

    private record BrevoSmsResponse(string? Reference);

    public Task<List<string>> SendPlainTextSmsAsync(IEnumerable<string>? recipients, string? body) =>
        SendPlainTextSmsAsync(recipients, body, null);

    public async Task<List<string>> SendPlainTextSmsAsync(
        IEnumerable<string>? recipients, string? body, string? tag)
    {
        var list = recipients?.ToList();
        if (list is null || list.Count == 0)
        {
            throw new ArgumentException("At least one recipient phone number is required");
        }
        if (string.IsNullOrWhiteSpace(body))
        {
            throw new ArgumentException("Rendered SMS body is empty");
        }

        var config = smsOptions.Value;
        var apiKey = TrimToNull(config.Brevo.ApiKey)
            ?? TrimToNull(emailOptions.Value.Brevo.ApiKey)
            ?? throw new InvalidOperationException("Brevo API key is not configured");

        var sender = TrimToNull(config.Sender)
            ?? throw new InvalidOperationException("Brevo SMS sender is not configured");

        var phones = list.Select(NormalizePhone).OfType<string>().ToList();
        if (phones.Count == 0)
        {
            throw new ArgumentException("At least one recipient phone number is required");
        }

        var type = ResolveType(config);
        var references = new List<string>();
        foreach (var phone in phones)
        {
            var message = new BrevoSmsRequest(
                sender,
                phone,
                body.Trim(),
                type,
                TrimToNull(tag) ?? TrimToNull(config.Tag),
                TrimToNull(config.WebUrl),
                config.UnicodeEnabled,
                TrimToNull(config.OrganisationPrefix));

            using var request = new HttpRequestMessage(HttpMethod.Post, "transactionalSMS/sms")
            {
                Content = JsonContent.Create(message, options: BrevoJson.Options),
            };
            request.Headers.Add("api-key", apiKey);

            using var response = await http.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync();
                throw new InvalidOperationException(
                    $"Unable to send SMS [HTTP {(int)response.StatusCode}]: {error}");
            }

            var result = await response.Content.ReadFromJsonAsync<BrevoSmsResponse>(BrevoJson.Options);
            var reference = TrimToNull(result?.Reference)
                ?? throw new InvalidOperationException("Brevo did not return an SMS reference");
            references.Add(reference);
        }
        return references;
    }

    private static string ResolveType(SmsOptions config)
    {
        var configured = TrimToNull(config.Type);
        if (configured is null)
        {
            return "transactional";
        }
        if (!SupportedTypes.Contains(configured))
        {
            throw new InvalidOperationException($"Unsupported Brevo SMS type: {configured}");
        }
        return configured;
    }

    private static string? NormalizePhone(string? phone)
    {
        var trimmed = TrimToNull(phone);
        if (trimmed is null)
        {
            return null;
        }
        var normalized = PhoneNoise().Replace(trimmed, "");
        if (normalized.StartsWith('+'))
        {
            normalized = normalized[1..];
        }
        if (TenDigits().IsMatch(normalized))
        {
            normalized = "1" + normalized;
        }
        if (!InternationalDigits().IsMatch(normalized))
        {
            throw new ArgumentException(
                "SMS recipient phone number must include country code and digits only");
        }
        return normalized;
    }

    private static string? TrimToNull(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    [GeneratedRegex(@"[\s().-]")]
    private static partial Regex PhoneNoise();

    [GeneratedRegex("^[0-9]{10}$")]
    private static partial Regex TenDigits();

    [GeneratedRegex("^[0-9]{8,15}$")]
    private static partial Regex InternationalDigits();
}
