import { isAxiosError } from 'axios';

// Formato de erro padrão da API (ProblemDetails)
interface ProblemDetails {
  title?: string;
  detail?: string;
  errors?: Record<string, string[]>;
}

// Transforma o erro da API em uma mensagem legível para o toast
export function mensagemDeErro(error: unknown, padrao: string): string {
  if (!isAxiosError<ProblemDetails>(error)) return padrao;
  if (!error.response) return 'Sem conexão com o servidor. Verifique se a API está rodando.';

  const dados = error.response.data;
  const primeiroErroDeCampo = dados?.errors ? Object.values(dados.errors).flat()[0] : undefined;

  return primeiroErroDeCampo ?? dados?.detail ?? dados?.title ?? padrao;
}
