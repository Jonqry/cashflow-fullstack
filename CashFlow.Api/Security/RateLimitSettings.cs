namespace CashFlow.Api.Security
{
    public static class RateLimitPolicies
    {
        // Política aplicada em /api/auth (login e cadastro)
        public const string Auth = "auth";
    }

    // Seção "RateLimit" do appsettings
    public sealed class RateLimitSettings
    {
        // Tentativas de login/cadastro por IP dentro da janela
        public int AuthPermitLimit { get; set; } = 5;
        public int AuthJanelaSegundos { get; set; } = 60;

        // Limite geral de requisições por IP por minuto (protege a API inteira contra abuso)
        public int GlobalPermitLimit { get; set; } = 100;
    }
}
