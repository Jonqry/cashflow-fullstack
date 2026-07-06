import { useState, useEffect } from 'react';
import { api } from '../../../infra/api';
import { toast } from 'react-toastify';
import type { Pessoa } from '../../pessoas/types';

interface CadastroTransacaoProps {
  onSuccess: () => void; // Função para avisar o App.tsx que deve atualizar a tela
}

export function CadastroTransacaoForm({ onSuccess }: CadastroTransacaoProps) {
  const [pessoas, setPessoas] = useState<Pessoa[]>([]);
  const [descricao, setDescricao] = useState('');
  const [valor, setValor] = useState('');
  const [tipo, setTipo] = useState<'Receita' | 'Despesa'>('Despesa');
  const [pessoaId, setPessoaId] = useState('');

  // Busca as pessoas assim que o componente carrega
  const carregarPessoas = async () => {
    try {
      const response = await api.get('/pessoas/totais');
      // Mesma rota de totais pois ela já traz as pessoas
      setPessoas(response.data.pessoas); 
    } catch (error) {
      toast.error('Erro ao buscar pessoas para o select.');
    }
  };

  useEffect(() => {
    carregarPessoas();
  }, []);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!descricao || !valor || !pessoaId) {
      toast.warn('Preencha todos os campos!');
      return;
    }

    try {
      await api.post('/transacoes', {
        descricao,
        valor: parseFloat(valor),
        tipo,
        pessoaId: parseInt(pessoaId)
      });

      toast.success('Transação registrada!');
      setDescricao('');
      setValor('');
      onSuccess(); // Atualiza a tabela
    } catch (error: any) {
      // Pega a mensagem de erro que vem lá do TransacaoService em C#
      const mensagemErro = error.response?.data || 'Erro desconhecido ao salvar transação.';
      toast.error(`Bloqueado: ${mensagemErro}`);
    }
  };

  return (
    <div style={{ border: '1px solid #ccc', padding: '20px', borderRadius: '8px', flex: 1 }}>
      <h3>Nova Transação</h3>
      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '15px' }}>
        
        <div>
          <label style={{ display: 'block', marginBottom: '5px' }}>Pessoa:</label>
          <select 
            value={pessoaId} 
            onChange={(e) => setPessoaId(e.target.value)}
            style={{ width: '100%', padding: '8px', borderRadius: '4px' }}
          >
            <option value="">Selecione uma pessoa</option>
            {pessoas.map(p => (
              <option key={p.id} value={p.id}>{p.nome}</option>
            ))}
          </select>
        </div>

        <div>
          <label style={{ display: 'block', marginBottom: '5px' }}>Descrição:</label>
          <input 
            type="text" value={descricao} onChange={(e) => setDescricao(e.target.value)} 
            placeholder="Ex: Conta de Luz"
            style={{ width: '100%', padding: '8px', borderRadius: '4px' }}
          />
        </div>

        <div style={{ display: 'flex', gap: '10px' }}>
          <div style={{ flex: 1 }}>
            <label style={{ display: 'block', marginBottom: '5px' }}>Valor (R$):</label>
            <input 
              type="number" step="0.01" value={valor} onChange={(e) => setValor(e.target.value)} 
              style={{ width: '100%', padding: '8px', borderRadius: '4px' }}
            />
          </div>
          <div style={{ flex: 1 }}>
            <label style={{ display: 'block', marginBottom: '5px' }}>Tipo:</label>
            <select 
              value={tipo} onChange={(e) => setTipo(e.target.value as 'Receita' | 'Despesa')}
              style={{ width: '100%', padding: '8px', borderRadius: '4px' }}
            >
              <option value="Despesa">Despesa</option>
              <option value="Receita">Receita</option>
            </select>
          </div>
        </div>

        <button type="submit" style={{ padding: '10px', backgroundColor: '#28a745', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' }}>
          Registrar Transação
        </button>
      </form>
    </div>
  );
}