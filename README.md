# CashFlow

Controle de gastos da casa. Cadastra as pessoas da família, registra receitas e despesas de cada uma e mostra o total por pessoa e o total geral.

## Tecnologias

- **API:** C# com .NET 10 (ASP.NET Core), Entity Framework Core e SQLite
- **Front-end:** React com TypeScript (Vite), Axios e React Toastify

## Decisões

1. **A regra de negócio fica no back-end.** Menor de 18 anos não pode registrar receita. A regra está no `TransacaoService`: a API responde `400 Bad Request` e o front-end só mostra o aviso. Assim a regra vale mesmo para quem chamar a API direto.
2. **Excluir uma pessoa apaga as transações dela.** Usei a exclusão em cascata do Entity Framework para não sobrar transação sem dono.
3. **O front-end é organizado por assunto.** As pastas `pessoas` e `transacoes` guardam tudo de cada parte, em vez de separar por tipo de arquivo.
4. **O banco é criado sozinho.** A API aplica as migrations ao iniciar, então basta rodar para testar.

## Como rodar

Precisa do Node.js e do .NET 10 SDK.

API:

```bash
cd CashFlow.Api
dotnet run
```

Front-end, em outro terminal:

```bash
cd CashFlow.Web
npm install
npm run dev
```

Depois abra o endereço que aparecer no terminal (normalmente http://localhost:5173).
