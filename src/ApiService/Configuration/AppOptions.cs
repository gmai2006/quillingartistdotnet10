namespace ApiService.Configuration;

public class StripeOptions
{
    public const string Section = "Stripe";

    public string SecretKey { get; set; } = string.Empty;
    public string SuccessUrl { get; set; } = string.Empty;
    public string CancelUrl { get; set; } = string.Empty;
}

public class BrevoOptions
{
    public string? ApiKey { get; set; }
}

public class EmailOptions
{
    public const string Section = "Email";

    public string FromAddress { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
    public string? ReplyTo { get; set; }
    public string SupportEmail { get; set; } = string.Empty;
    public BrevoOptions Brevo { get; set; } = new();
}

public class SmsOptions
{
    public const string Section = "Sms";

    public string? Sender { get; set; }
    public string? Type { get; set; }
    public bool UnicodeEnabled { get; set; }
    public string? Tag { get; set; }
    public string? WebUrl { get; set; }
    public string? OrganisationPrefix { get; set; }
    public BrevoOptions Brevo { get; set; } = new();
}

public class JwtOptions
{
    public const string Section = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SigningKey { get; set; } = string.Empty;
}
