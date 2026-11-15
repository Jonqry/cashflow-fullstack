using System.ComponentModel.DataAnnotations;

namespace CashFlow.Api.Dtos
{
    public sealed class RegistrarRequest
    {
        [Required(ErrorMessage = "Informe o e-mail.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        [StringLength(254, ErrorMessage = "E-mail muito longo.")]
        public string Email { get; init; } = string.Empty;

        // Mínimo de 8 caracteres e teto de 128 para evitar payloads gigantes no hash
        [Required(ErrorMessage = "Informe a senha.")]
        [StringLength(128, MinimumLength = 8, ErrorMessage = "A senha deve ter entre 8 e 128 caracteres.")]
        public string Senha { get; init; } = string.Empty;
    }

    public sealed class LoginRequest
    {
        [Required(ErrorMessage = "Informe o e-mail.")]
        [StringLength(254)]
        public string Email { get; init; } = string.Empty;

        [Required(ErrorMessage = "Informe a senha.")]
        [StringLength(128)]
        public string Senha { get; init; } = string.Empty;
    }

    public sealed record UsuarioResponse(int Id, string Email);

    public sealed record TokenResponse(string Token, DateTime ExpiraEm);
}
