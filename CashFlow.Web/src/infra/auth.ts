// Guarda o token JWT no sessionStorage: some quando a aba é fechada.
// Escolha consciente: é mais simples que cookie httpOnly, mas um XSS conseguiria ler o token.
// Por isso o React não usa dangerouslySetInnerHTML e a sessão expira em 60 minutos.
const CHAVE_TOKEN = 'cashflow.token';
const EVENTO_SESSAO = 'cashflow:sessao';

export function obterToken(): string | null {
  return sessionStorage.getItem(CHAVE_TOKEN);
}

export function salvarToken(token: string) {
  sessionStorage.setItem(CHAVE_TOKEN, token);
  window.dispatchEvent(new Event(EVENTO_SESSAO));
}

export function encerrarSessao() {
  sessionStorage.removeItem(CHAVE_TOKEN);
  window.dispatchEvent(new Event(EVENTO_SESSAO));
}

// Avisa quem estiver ouvindo (o App) quando a pessoa entra ou sai
export function aoMudarSessao(callback: () => void) {
  window.addEventListener(EVENTO_SESSAO, callback);
  return () => window.removeEventListener(EVENTO_SESSAO, callback);
}
