using System.ComponentModel.DataAnnotations;

namespace CashFlow.Api.Dtos
{
    // DTO de entrada: só aceita os campos que o cliente pode definir.
    // Antes a API recebia a entidade Pessoa inteira, o que permitia enviar
    // Id e uma lista de Transacoes junto (mass assignment).
    public sealed class PessoaRequest
    {
        // Lista de permissão: letras (com acento), espaço, apóstrofo, ponto e hífen
        [Required(ErrorMessage = "Informe o nome.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 100 caracteres.")]
        [RegularExpression(@"^[\p{L}\p{M}' .-]+$", ErrorMessage = "O nome contém caracteres inválidos.")]
        public string Nome { get; init; } = string.Empty;

        [Range(0, 130, ErrorMessage = "A idade deve estar entre 0 e 130.")]
        public int Idade { get; init; }
    }

    public sealed record PessoaResponse(int Id, string Nome, int Idade);
}
