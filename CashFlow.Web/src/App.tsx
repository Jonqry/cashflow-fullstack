import { useState } from 'react';
import { ToastContainer } from 'react-toastify';
import 'react-toastify/dist/ReactToastify.css';

// Importação dos componentes da estrutura de Features
import { CadastroPessoaForm } from './features/pessoas/components/CadastroPessoaForm';
import { CadastroTransacaoForm } from './features/transacoes/components/CadastroTransacaoForm';
import { ConsultaTotais } from './features/pessoas/components/ConsultaTotais';

export default function App() {
  // Estado utilizado para forçar a re-renderização da tabela de consulta
  const [refreshKey, setRefreshKey] = useState(0);

  // Callback propagado para os componentes filhos atualizarem a listagem após mutações
  const handleAtualizarDados = () => {
    setRefreshKey((oldKey) => oldKey + 1);
  };

  return (
    <div style={{ backgroundColor: '#f8f9fa', minHeight: '100vh', padding: '40px 20px', fontFamily: 'sans-serif' }}>
      <div style={{ maxWidth: '1100px', margin: '0 auto' }}>
        <h1 style={{ textAlign: 'center', color: '#2c3e50', marginBottom: '40px', fontWeight: 'bold' }}>
          Sistema de Controle de Gastos
        </h1>
        
        {/* Container global de notificações */}
        <ToastContainer position="top-right" autoClose={4000} />

        <main style={{ display: 'flex', flexDirection: 'column', gap: '30px' }}>
          
          {/* Container superior: Formulários de entrada */}
          <div style={{ display: 'flex', gap: '20px', flexWrap: 'wrap' }}>
            <div style={{ flex: 1, minWidth: '300px' }}>
              <CadastroPessoaForm onSuccess={handleAtualizarDados} />
            </div>
            
            <div style={{ flex: 1, minWidth: '300px' }}>
              <CadastroTransacaoForm onSuccess={handleAtualizarDados} refreshKey={refreshKey} />
            </div>
          </div>

          {/* Container inferior: Tabela de totais */}
          <div style={{ width: '100%' }}>
            <ConsultaTotais refreshKey={refreshKey} onActionSuccess={handleAtualizarDados} />
          </div>
          
        </main>
      </div>
    </div>
  );
}