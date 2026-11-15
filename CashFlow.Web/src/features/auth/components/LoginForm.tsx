import { useState } from 'react';
import { toast } from 'react-toastify';
import { api } from '../../../infra/api';
import { salvarToken } from '../../../infra/auth';
import { mensagemDeErro } from '../../../infra/erros';

const estiloInput = { width: '100%', padding: '10px', borderRadius: '6px', border: '1px solid #e0e0e0', boxSizing: 'border-box' as const };
const estiloLabel = { display: 'block', marginBottom: '6px', fontSize: '14px', color: '#555' };

export function LoginForm() {
  const [modo, setModo] = useState<'entrar' | 'cadastrar'>('entrar');
  const [email, setEmail] = useState('');
  const [senha, setSenha] = useState('');
  const [enviando, setEnviando] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!email || !senha) {
      toast.warn('Preencha e-mail e senha.');
      return;
    }

    setEnviando(true);
    try {
      if (modo === 'cadastrar') {
        await api.post('/auth/registrar', { email, senha });
        toast.success('Conta criada!');
      }

      const { data } = await api.post<{ token: string }>('/auth/login', { email, senha });
      setSenha('');
      salvarToken(data.token);
    } catch (error) {
      toast.error(mensagemDeErro(error, 'Não foi possível entrar.'));
    } finally {
      setEnviando(false);
    }
  };

  return (
    <div style={{ backgroundColor: '#ffffff', padding: '24px', borderRadius: '12px', boxShadow: '0 4px 12px rgba(0,0,0,0.05)', maxWidth: '380px', margin: '0 auto' }}>
      <h3 style={{ marginTop: 0, color: '#333' }}>{modo === 'entrar' ? 'Entrar' : 'Criar conta'}</h3>
      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '15px' }}>
        <div>
          <label style={estiloLabel}>E-mail:</label>
          <input type="email" autoComplete="username" value={email} onChange={(e) => setEmail(e.target.value)} maxLength={254} style={estiloInput} />
        </div>

        <div>
          <label style={estiloLabel}>Senha:</label>
          <input
            type="password"
            autoComplete={modo === 'entrar' ? 'current-password' : 'new-password'}
            value={senha}
            onChange={(e) => setSenha(e.target.value)}
            minLength={modo === 'cadastrar' ? 8 : undefined}
            maxLength={128}
            style={estiloInput}
          />
          {modo === 'cadastrar' && <small style={{ color: '#777' }}>Mínimo de 8 caracteres.</small>}
        </div>

        <button
          type="submit"
          disabled={enviando}
          style={{ padding: '12px', backgroundColor: '#007BFF', color: 'white', border: 'none', borderRadius: '6px', cursor: 'pointer', fontWeight: 'bold', opacity: enviando ? 0.7 : 1 }}
        >
          {modo === 'entrar' ? 'Entrar' : 'Criar conta e entrar'}
        </button>

        <button
          type="button"
          onClick={() => setModo(modo === 'entrar' ? 'cadastrar' : 'entrar')}
          style={{ background: 'none', border: 'none', color: '#007BFF', cursor: 'pointer', fontSize: '14px' }}
        >
          {modo === 'entrar' ? 'Não tem conta? Cadastre-se' : 'Já tenho conta'}
        </button>
      </form>
    </div>
  );
}
