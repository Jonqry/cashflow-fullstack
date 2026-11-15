import { useState } from 'react';
import { api } from '../../../infra/api';
import { toast } from 'react-toastify';
import { mensagemDeErro } from '../../../infra/erros';

export function CadastroPessoaForm({ onSuccess }: { onSuccess: () => void }) {
  const [nome, setNome] = useState('');
  const [idade, setIdade] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault(); // Intercepta o envio padrão para evitar recarregamento da página

    if (!nome || !idade) {
      toast.warn('Preencha todos os campos obrigatórios!');
      return;
    }

    try {
      // Transmissão da carga de dados para a API
      await api.post('/pessoas', { 
        nome, 
        idade: parseInt(idade) 
      });

      toast.success('Pessoa cadastrada com sucesso!');
      
      // Reset do estado local do formulário
      setNome('');
      setIdade('');

      // Dispara o callback para atualização da interface externa
      onSuccess();
      
    } catch (error) {
      // Mostra a mensagem de validação devolvida pela API (ex.: nome com caracteres inválidos)
      toast.error(mensagemDeErro(error, 'Não foi possível cadastrar a pessoa.'));
    }
  };

  return (
    <div style={{ backgroundColor: '#ffffff', padding: '24px', borderRadius: '12px', boxShadow: '0 4px 12px rgba(0,0,0,0.05)', flex: 1, minWidth: '300px' }}>
      <h3 style={{ marginTop: 0, color: '#333' }}>Nova Pessoa</h3>
      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '15px' }}>
        
        <div>
          <label style={{ display: 'block', marginBottom: '6px', fontSize: '14px', color: '#555' }}>Nome completo:</label>
          <input 
            type="text" 
            value={nome} 
            onChange={(e) => setNome(e.target.value)} 
            placeholder="Ex: João Victor"
            maxLength={100}
            style={{ width: '100%', padding: '10px', borderRadius: '6px', border: '1px solid #e0e0e0', boxSizing: 'border-box' }}
          />
        </div>

        <div>
          <label style={{ display: 'block', marginBottom: '6px', fontSize: '14px', color: '#555' }}>Idade:</label>
          <input 
            type="number" 
            value={idade} 
            onChange={(e) => setIdade(e.target.value)} 
            placeholder="Ex: 25"
            style={{ width: '100%', padding: '10px', borderRadius: '6px', border: '1px solid #e0e0e0', boxSizing: 'border-box' }}
          />
        </div>

        <button 
          type="submit" 
          style={{ padding: '12px', backgroundColor: '#007BFF', color: 'white', border: 'none', borderRadius: '6px', cursor: 'pointer', fontWeight: 'bold', marginTop: '5px' }}
        >
          Cadastrar Pessoa
        </button>
      </form>
    </div>
  );
}