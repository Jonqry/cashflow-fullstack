namespace CashFlow.Api.Security
{
    // Valores lidos da seção "Jwt" do appsettings. A chave (Jwt:Key) NÃO fica no repositório:
    // use dotnet user-secrets em desenvolvimento ou a variável de ambiente Jwt__Key em produção.
    public sealed class JwtSettings
    {
        public string Issuer { get; set; } = "CashFlow.Api";
        public string Audience { get; set; } = "CashFlow.Web";
        public int ExpiracaoMinutos { get; set; } = 60;
        public string? Key { get; set; }
    }
}
