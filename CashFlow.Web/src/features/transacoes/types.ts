export interface Transacao {
  id?: number;
  descricao: string;
  valor: number;
  tipo: 'Receita' | 'Despesa';
  pessoaId: number;
}