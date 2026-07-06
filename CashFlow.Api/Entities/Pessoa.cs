namespace CashFlow.Api.Entities
{
    public class Pessoa
    {
        public int Id { get; set; } 
        public string Nome { get; set; } = string.Empty;
        public int Idade { get; set; }
        
        // Relação: Uma pessoa pode ter uma lista de transações atreladas a ela
        public List<Transacao> Transacoes { get; set; } = new List<Transacao>();
    }
}