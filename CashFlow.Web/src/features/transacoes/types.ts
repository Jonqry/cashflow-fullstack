export interface Transacao {
  id?: number; // Opcional porque ao criar ainda não temos ID
  descricao: string;
  valor: number;
  tipo: 'Receita' | 'Despesa';
  pessoaId: number;
}