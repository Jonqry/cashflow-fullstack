import { useState } from 'react';
import { ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

// Importando os 3 componentes da estrutura de Features
import { CadastroPessoaForm } from './features/pessoas/components/CadastroPessoaForm';
import { CadastroTransacaoForm } from './features/transacoes/components/CadastroTransacaoForm';
import { ConsultaTotais } from './features/pessoas/components/ConsultaTotais';

export default function App() {
  // Estado que funciona como um "gatilho" para a tabela recarregar
  const [refreshKey, setRefreshKey] = useState(0);

  const handleAtualizarDados = () => {
    setRefreshKey((oldKey) => oldKey + 1);
  };

  return (
    <div style={{ padding: '40px', fontFamily: 'sans-serif', maxWidth: '1100px', margin: '0 auto' }}>
      <h1 style={{ textAlign: 'center', color: '#2c3e50', marginBottom: '40px' }}>
        Sistema de Controle de Gastos
      </h1>
      
      {/* Container de notificações */}
      <ToastContainer position="top-right" autoClose={4000} />

      <main style={{ display: 'flex', flexDirection: 'column', gap: '40px' }}>
        
        {/* Bloco Superior: Os dois formulários lado a lado */}
        <div style={{ display: 'flex', gap: '20px', flexWrap: 'wrap' }}>
          {/* O flex: 1 faz com que eles dividam o espaço igualmente */}
          <div style={{ flex: 1, minWidth: '300px' }}>
            <CadastroPessoaForm onSuccess={handleAtualizarDados} />
          </div>
          
          <div style={{ flex: 1, minWidth: '300px' }}>
            <CadastroTransacaoForm onSuccess={handleAtualizarDados} />
          </div>
        </div>

        {/* Bloco Inferior: A tabela de totais ocupando toda a largura */}
        <div style={{ width: '100%' }}>
          <ConsultaTotais refreshKey={refreshKey} />
        </div>
        
      </main>
    </div>
  );
}