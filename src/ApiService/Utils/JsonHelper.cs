using System.Globalization;

namespace ApiService.Utils;

public static class JsonHelper
{
    private static readonly TimeZoneInfo LosAngeles =
        TimeZoneInfo.FindSystemTimeZoneById("America/Los_Angeles");

    public static string CsvValue(object? value)
    {
        if (value is null)
        {
            return "\"\"";
        }

        var text = value switch
        {
            bool b => b ? "true" : "false",
            DateTime dt => dt.ToString("yyyy-MM-ddTHH:mm:ss.FFFFFFF", CultureInfo.InvariantCulture),
            DateTimeOffset dto => dto.ToString("o", CultureInfo.InvariantCulture),
            IFormattable f => f.ToString(null, CultureInfo.InvariantCulture),
            _ => value.ToString() ?? string.Empty,
        };
        return "\"" + text.Replace("\"", "\"\"") + "\"";
    }

    public static DateTimeOffset? ParsePerformed(string? raw, bool isEnd)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;

        var s = raw.Trim();

        if (s.Length == 10 && s[4] == '-' && s[7] == '-'
            && DateOnly.TryParseExact(s, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
        {
            var local = isEnd ? d.ToDateTime(new TimeOnly(23, 59, 59)) : d.ToDateTime(TimeOnly.MinValue);
            return AtLosAngeles(local);
        }

        var hasOffset = s.EndsWith('Z') || s.EndsWith('z') || s.LastIndexOfAny(['+', '-']) > 10;
        if (hasOffset && DateTimeOffset.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var dto))
        {
            return dto;
        }

        if (DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out var ldt))
        {
            return AtLosAngeles(DateTime.SpecifyKind(ldt, DateTimeKind.Unspecified));
        }

        throw new ArgumentException(
            "Invalid performedFrom/performedTo. Use ISO-8601 (e.g. 2026-02-27T10:15:00-08:00 or 2026-02-27).");
    }

    public static DateTime? ToLocalTimestamp(DateTimeOffset? value) =>
        value is null ? null : DateTime.SpecifyKind(value.Value.LocalDateTime, DateTimeKind.Unspecified);

    public static DateTimeOffset? ToUtc(DateTimeOffset? value) => value?.ToUniversalTime();

    private static DateTimeOffset AtLosAngeles(DateTime local) =>
        new(local, LosAngeles.GetUtcOffset(local));
}
