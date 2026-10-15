using System.Text.Json;
using System.Text.Json.Serialization;

namespace ApiService.Services.Communications;

internal static class BrevoJson
{
    public const string BaseUrl = "https://api.brevo.com/v3/";

    public static readonly JsonSerializerOptions Options = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    };
}
