namespace CashFlow.Api.Entities
{
    public class Usuario
    {
        public int Id { get; set; }

        // Sempre salvo normalizado (minúsculo e sem espaços) para o índice único funcionar
        public string Email { get; set; } = string.Empty;

        // Hash PBKDF2 gerado pelo PasswordHasher do ASP.NET Core. A senha em texto puro nunca é salva.
        public string SenhaHash { get; set; } = string.Empty;
    }
}
