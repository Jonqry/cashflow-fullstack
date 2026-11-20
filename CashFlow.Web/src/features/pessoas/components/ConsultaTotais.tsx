import { useState, useEffect } from 'react';
import { api } from '../../../infra/api';
import type { PessoaTotal } from '../types';
import { toast } from 'react-toastify';

const reais = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' });

interface ConsultaTotaisProps {
  refreshKey: number;
  onActionSuccess: () => void; // Callback para propagar atualizações de estado
}

interface DadosGerais {
  pessoas: PessoaTotal[];
  totalGeralReceitas: number;
  totalGeralDespesas: number;
  saldoLiquidoGeral: number;
}

export function ConsultaTotais({ refreshKey, onActionSuccess }: ConsultaTotaisProps) {
  const [dados, setDados] = useState<DadosGerais | null>(null);

  // Efeito colateral responsável por sincronizar a listagem de totais com o servidor
  useEffect(() => {
    const buscarTotais = async () => {
      try {
        const response = await api.get('/pessoas/totais');
        setDados(response.data);
      } catch (error) {
        toast.error('Erro ao processar a tabela de dados.');
      }
    };
    buscarTotais();
  }, [refreshKey]);

  // Função responsável por solicitar a deleção do registro e acionar o cascade delete
  const handleDelete = async (id: number, nome: string) => {
    const confirmar = window.confirm(`Tem certeza que deseja excluir ${nome}? A ação removerá permanentemente as transações vinculadas.`);
    
    if (confirmar) {
      try {
        await api.delete(`/pessoas/${id}`);
        toast.success(`${nome} removido(a) com sucesso.`);
        onActionSuccess(); 
      } catch (error) {
        toast.error('Erro ao processar a exclusão no servidor.');
      }
    }
  };

  if (!dados) return <p style={{ color: '#555' }}>Processando dados...</p>;

  return (
    <div style={{ backgroundColor: '#ffffff', padding: '24px', borderRadius: '12px', boxShadow: '0 4px 12px rgba(0,0,0,0.05)' }}>
      <h3 style={{ marginTop: 0, color: '#333' }}>Consulta de Totais</h3>
      <div style={{ overflowX: 'auto' }}>
        <table style={{ width: '100%', borderCollapse: 'collapse', marginTop: '10px' }}>
          <thead>
            <tr style={{ backgroundColor: '#f8f9fa', textAlign: 'left' }}>
              <th style={{ padding: '12px', borderBottom: '2px solid #eee', color: '#555' }}>Nome do Responsável</th>
              <th style={{ padding: '12px', borderBottom: '2px solid #eee', color: '#555' }}>Receitas</th>
              <th style={{ padding: '12px', borderBottom: '2px solid #eee', color: '#555' }}>Despesas</th>
              <th style={{ padding: '12px', borderBottom: '2px solid #eee', color: '#555' }}>Saldo Líquido</th>
              <th style={{ padding: '12px', borderBottom: '2px solid #eee', color: '#555', textAlign: 'center' }}>Ações</th>
            </tr>
          </thead>
          <tbody>
            {dados.pessoas.map((pessoa) => (
              <tr key={pessoa.id} style={{ transition: 'background-color 0.2s' }}>
                <td style={{ padding: '12px', borderBottom: '1px solid #eee' }}>{pessoa.nome}</td>
                <td style={{ padding: '12px', borderBottom: '1px solid #eee', color: '#28a745', fontWeight: '500' }}>
                  {reais.format(pessoa.totalReceitas)}
                </td>
                <td style={{ padding: '12px', borderBottom: '1px solid #eee', color: '#dc3545', fontWeight: '500' }}>
                  {reais.format(pessoa.totalDespesas)}
                </td>
                <td style={{ padding: '12px', borderBottom: '1px solid #eee', fontWeight: 'bold', color: pessoa.saldo >= 0 ? '#28a745' : '#dc3545' }}>
                  {reais.format(pessoa.saldo)}
                </td>
                <td style={{ padding: '12px', borderBottom: '1px solid #eee', textAlign: 'center' }}>
                  <button 
                    onClick={() => handleDelete(pessoa.id, pessoa.nome)}
                    style={{ padding: '6px 12px', backgroundColor: '#dc3545', color: 'white', border: 'none', borderRadius: '4px', cursor: 'pointer', fontSize: '13px', fontWeight: 'bold' }}
                  >
                    Excluir
                  </button>
                </td>
              </tr>
            ))}
          </tbody>
          <tfoot>
            <tr style={{ backgroundColor: '#f8f9fa' }}>
              <td style={{ padding: '15px 12px', fontWeight: 'bold', color: '#333' }}>TOTAIS GERAIS</td>
              <td style={{ padding: '15px 12px', color: '#28a745', fontWeight: 'bold' }}>{reais.format(dados.totalGeralReceitas)}</td>
              <td style={{ padding: '15px 12px', color: '#dc3545', fontWeight: 'bold' }}>{reais.format(dados.totalGeralDespesas)}</td>
              <td style={{ padding: '15px 12px', fontWeight: 'bold', fontSize: '16px', color: dados.saldoLiquidoGeral >= 0 ? '#28a745' : '#dc3545' }}>
                {reais.format(dados.saldoLiquidoGeral)}
              </td>
              <td></td>
            </tr>
          </tfoot>
        </table>
      </div>
    </div>
  );
}