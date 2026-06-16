namespace SkyRoute.Infrastructure.Authentication;

public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = "SkyRoute";
    public string Audience { get; set; } = "SkyRoute";
    public int ExpirationHours { get; set; } = 1;
}
