import { useState, useEffect } from 'react';
import { api } from '../../../infra/api';
import type { PessoaTotal } from '../types';
import { toast } from 'react-toastify';

interface ConsultaTotaisProps {
  refreshKey: number; // Quando este número mudar, o useEffect recarrega os dados
}

interface DadosGerais {
  pessoas: PessoaTotal[];
  totalGeralReceitas: number;
  totalGeralDespesas: number;
  saldoLiquidoGeral: number;
}

export function ConsultaTotais({ refreshKey }: ConsultaTotaisProps) {
  const [dados, setDados] = useState<DadosGerais | null>(null);

  useEffect(() => {
    const buscarTotais = async () => {
      try {
        const response = await api.get('/pessoas/totais');
        setDados(response.data);
      } catch (error) {
        toast.error('Erro ao buscar a tabela de totais.');
      }
    };
    buscarTotais();
  }, [refreshKey]);

  if (!dados) return <p>Carregando tabela...</p>;

  return (
    <div style={{ border: '1px solid #ccc', padding: '20px', borderRadius: '8px' }}>
      <h3>Consulta de Totais</h3>
      <table style={{ width: '100%', borderCollapse: 'collapse', marginTop: '15px' }}>
        <thead>
          <tr style={{ backgroundColor: '#f4f4f4', textAlign: 'left' }}>
            <th style={{ padding: '10px', borderBottom: '1px solid #ddd' }}>Nome</th>
            <th style={{ padding: '10px', borderBottom: '1px solid #ddd' }}>Receitas</th>
            <th style={{ padding: '10px', borderBottom: '1px solid #ddd' }}>Despesas</th>
            <th style={{ padding: '10px', borderBottom: '1px solid #ddd' }}>Saldo</th>
          </tr>
        </thead>
        <tbody>
          {dados.pessoas.map((pessoa) => (
            <tr key={pessoa.id}>
              <td style={{ padding: '10px', borderBottom: '1px solid #eee' }}>{pessoa.nome}</td>
              <td style={{ padding: '10px', borderBottom: '1px solid #eee', color: '#28a745' }}>
                R$ {pessoa.totalReceitas.toFixed(2)}
              </td>
              <td style={{ padding: '10px', borderBottom: '1px solid #eee', color: '#dc3545' }}>
                R$ {pessoa.totalDespesas.toFixed(2)}
              </td>
              <td style={{ padding: '10px', borderBottom: '1px solid #eee', fontWeight: 'bold', color: pessoa.saldo >= 0 ? '#28a745' : '#dc3545' }}>
                R$ {pessoa.saldo.toFixed(2)}
              </td>
            </tr>
          ))}
        </tbody>
        <tfoot>
          <tr style={{ backgroundColor: '#f8f9fa', fontWeight: 'bold' }}>
            <td style={{ padding: '10px' }}>TOTAIS GERAIS</td>
            <td style={{ padding: '10px', color: '#28a745' }}>R$ {dados.totalGeralReceitas.toFixed(2)}</td>
            <td style={{ padding: '10px', color: '#dc3545' }}>R$ {dados.totalGeralDespesas.toFixed(2)}</td>
            <td style={{ padding: '10px', color: dados.saldoLiquidoGeral >= 0 ? '#28a745' : '#dc3545' }}>
              R$ {dados.saldoLiquidoGeral.toFixed(2)}
            </td>
          </tr>
        </tfoot>
      </table>
    </div>
  );
}