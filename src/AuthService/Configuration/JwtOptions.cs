namespace AuthService.Configuration;

public class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string PrivateKeyPath { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; }
    public string PublicKeyPath { get; set; } = string.Empty;
}