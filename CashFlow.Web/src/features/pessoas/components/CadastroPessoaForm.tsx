import { useState } from 'react';
import { api } from '../../../infra/api';
import { toast } from 'react-toastify';

export function CadastroPessoaForm({ onSuccess }: { onSuccess: () => void }) {
  const [nome, setNome] = useState('');
  const [idade, setIdade] = useState('');

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault(); // Evita que a página recarregue ao enviar o form

    if (!nome || !idade) {
      toast.warn('Preencha todos os campos!');
      return;
    }

    try {
      // Fazendo a chamada POST para a API
      await api.post('/pessoas', { 
        nome, 
        idade: parseInt(idade) 
      });

      toast.success('Pessoa cadastrada com sucesso!');
      
      // Limpa os campos após o sucesso
      setNome('');
      setIdade('');

      // Avisa o App.tsx para recarregar a tabela
      onSuccess();
      
    } catch (error) {
      console.error(error);
      toast.error('Erro ao conectar com a API. Verifique se o back-end está rodando.');
    }
  };

  return (
    <div style={{ border: '1px solid #ccc', padding: '20px', borderRadius: '8px', flex: 1, minWidth: '300px' }}>
      <h3>Nova Pessoa</h3>
      <form onSubmit={handleSubmit} style={{ display: 'flex', flexDirection: 'column', gap: '15px' }}>
        
        <div>
          <label style={{ display: 'block', marginBottom: '5px' }}>Nome:</label>
          <input 
            type="text" 
            value={nome} 
            onChange={(e) => setNome(e.target.value)} 
            placeholder="Ex: João Victor"
            style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ccc', boxSizing: 'border-box' }}
          />
        </div>

        <div>
          <label style={{ display: 'block', marginBottom: '5px' }}>Idade:</label>
          <input 
            type="number" 
            value={idade} 
            onChange={(e) => setIdade(e.target.value)} 
            placeholder="Ex: 25"
            style={{ width: '100%', padding: '8px', borderRadius: '4px', border: '1px solid #ccc', boxSizing: 'border-box' }}
          />
        </div>

        <button 
          type="submit" 
          style={{ padding: '10px', backgroundColor: '#007BFF', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer' }}
        >
          Cadastrar Pessoa
        </button>
      </form>
    </div>
  );
}