import { useState, useEffect } from 'react';
import { api } from '../../../infra/api';
import { toast } from 'react-toastify';
import type { Pessoa } from '../../pessoas/types';

interface CadastroTransacaoProps {
  onSuccess: () => void; // Callback para notificar atualizações ao componente pai
  refreshKey: number;    // Estado de controle para disparar a sincronização dos dados do seletor
}

export function CadastroTransacaoForm({ onSuccess, refreshKey }: CadastroTransacaoProps) {
  const [pessoas, setPessoas] = useState<Pessoa[]>([]);
  const [descricao, setDescricao] = useState('');
  const [valor, setValor] = useState('');
  const [tipo, setTipo] = useState<'Receita' | 'Despesa'>('Despesa');
  const [pessoaId, setPessoaId] = useState('');

  // Consulta a listagem de pessoas disponíveis para sincronização do campo de seleção
  const carregarPessoas = async () => {
    try {
      const response = await api.get('/pessoas/totais');
      setPessoas(response.data.pessoas); 
    } catch (error) {
      toast.error('Erro ao buscar a listagem de pessoas para o seletor.');
    }
  };

  // Monitora alterações no refreshKey para atualizar a lista sempre que houver cadastros ou exclusões
  useEffect(() => {
    carregarPessoas();
  }, [refreshKey]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!descricao || !valor || !pessoaId) {
      toast.warn('Preencha todos os campos obrigatórios!');
      return;
    }

    try {
      await api.post('/transacoes', {
        descricao,
        valor: parseFloat(valor),
        tipo,
        pessoaId: parseInt(pessoaId)
      });

      toast.success('Transação registrada com sucesso!');
      setDescricao('');
      setValor('');
      
      // Executa o callback de sucesso para atualizar a interface geral
      onSuccess(); 
    } catch (error: any) {
      // Captura e exibe a mensagem de validação retornada pela camada de serviço do servidor
      const mensagemErro = error.response?.data || 'Erro interno ao processar transação.';
      toast.error(`Bloqueado: ${mensagemErro}`);
    }
  };

  return (
    <div style={{ backgroundColor: '#ffffff', padding: '24px', borderRadius: '12px', boxShadow: '0 4px 12px rgba(0,0,0,0.05)', flex: 1 }}>
      <h3 style={{ marginTop: 0, color: '#333' }}>Nova Transação</h3>
      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '15px' }}>
        
        <div>
          <label style={{ display: 'block', marginBottom: '6px', fontSize: '14px', color: '#555' }}>Pessoa responsável:</label>
          <select 
            value={pessoaId} 
            onChange={(e) => setPessoaId(e.target.value)}
            style={{ width: '100%', padding: '10px', borderRadius: '6px', border: '1px solid #e0e0e0', boxSizing: 'border-box' }}
          >
            <option value="">Selecione um responsável</option>
            {pessoas.map(p => (
              <option key={p.id} value={p.id}>{p.nome}</option>
            ))}
          </select>
        </div>

        <div>
          <label style={{ display: 'block', marginBottom: '6px', fontSize: '14px', color: '#555' }}>Descrição da transação:</label>
          <input 
            type="text" 
            value={descricao} 
            onChange={(e) => setDescricao(e.target.value)} 
            placeholder="Ex: Conta de Luz"
            style={{ width: '100%', padding: '10px', borderRadius: '6px', border: '1px solid #e0e0e0', boxSizing: 'border-box' }}
          />
        </div>

        <div style={{ display: 'flex', gap: '10px' }}>
          <div style={{ flex: 1 }}>
            <label style={{ display: 'block', marginBottom: '6px', fontSize: '14px', color: '#555' }}>Valor (R$):</label>
            <input 
              type="number" 
              step="0.01" 
              value={valor} 
              onChange={(e) => setValor(e.target.value)} 
              style={{ width: '100%', padding: '10px', borderRadius: '6px', border: '1px solid #e0e0e0', boxSizing: 'border-box' }}
            />
          </div>
          <div style={{ flex: 1 }}>
            <label style={{ display: 'block', marginBottom: '6px', fontSize: '14px', color: '#555' }}>Tipo:</label>
            <select 
              value={tipo} 
              onChange={(e) => setTipo(e.target.value as 'Receita' | 'Despesa')}
              style={{ width: '100%', padding: '10px', borderRadius: '6px', border: '1px solid #e0e0e0', boxSizing: 'border-box' }}
            >
              <option value="Despesa">Despesa</option>
              <option value="Receita">Receita</option>
            </select>
          </div>
        </div>

        <button 
          type="submit" 
          style={{ padding: '12px', backgroundColor: '#28a745', color: 'white', border: 'none', borderRadius: '6px', cursor: 'pointer', fontWeight: 'bold', marginTop: '5px' }}
        >
          Registrar Transação
        </button>
      </form>
    </div>
  );
}