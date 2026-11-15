using System.ComponentModel.DataAnnotations;
using CashFlow.Api.Entities;

namespace CashFlow.Api.Dtos
{
    public sealed class TransacaoRequest : IValidatableObject
    {
        [Required(ErrorMessage = "Informe a descrição.")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "A descrição deve ter entre 2 e 200 caracteres.")]
        public string Descricao { get; init; } = string.Empty;

        // Limites lidos em cultura invariável para não depender do idioma do servidor (pt-BR usa vírgula)
        [Range(typeof(decimal), "0.01", "999999999.99",
            ParseLimitsInInvariantCulture = true,
            ConvertValueInInvariantCulture = true,
            ErrorMessage = "O valor deve ser maior que zero e menor que 1 bilhão.")]
        public decimal Valor { get; init; }

        [Required(ErrorMessage = "Informe o tipo.")]
        [AllowedValues(TiposTransacao.Receita, TiposTransacao.Despesa, ErrorMessage = "O tipo deve ser 'Receita' ou 'Despesa'.")]
        public string Tipo { get; init; } = string.Empty;

        [Range(1, int.MaxValue, ErrorMessage = "Informe uma pessoa válida.")]
        public int PessoaId { get; init; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            // Dinheiro com no máximo 2 casas decimais
            if (decimal.Round(Valor, 2) != Valor)
            {
                yield return new ValidationResult("O valor deve ter no máximo 2 casas decimais.", [nameof(Valor)]);
            }
        }
    }

    public sealed record TransacaoResponse(int Id, string Descricao, decimal Valor, string Tipo, int PessoaId);
}
