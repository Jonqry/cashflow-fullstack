namespace CashFlow.Api.Entities
{
    public class Transacao
    {
        public int Id { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string Tipo { get; set; } = string.Empty; // Vai receber "Receita" ou "Despesa"
        
        // Chave Estrangeira obrigatória (isso garante o Cascade Delete do desafio)
        public int PessoaId { get; set; }
        public Pessoa? Pessoa { get; set; } 
    }
}