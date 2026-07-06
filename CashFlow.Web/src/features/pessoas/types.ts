// Representa a pessoa na hora de cadastrar
export interface Pessoa {
  id: number;
  nome: string;
  idade: number;
}

// Representa a pessoa com os cálculos matemáticos já feitos (para a tabela)
export interface PessoaTotal {
  id: number;
  nome: string;
  totalReceitas: number;
  totalDespesas: number;
  saldo: number;
}