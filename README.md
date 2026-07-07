# 💰 CashFlow - Sistema de Gestão Financeira

Bem-vindo ao repositório do **CashFlow**! Este é um sistema Full-Stack desenvolvido para facilitar o controle de finanças pessoais, permitindo o cadastro de usuários e o registro de suas respectivas despesas e receitas.

O objetivo central deste projeto foi construir uma aplicação corporativa robusta, onde o Back-end fosse rigoroso na proteção das regras de negócio, e o Front-end entregasse uma experiência fluida, reativa e de alto desempenho.

---

## 🛠️ Tecnologias Utilizadas

Para garantir uma arquitetura escalável e alinhada às exigências do mercado, o projeto foi segmentado com clara separação de responsabilidades:

**Back-end (`/CashFlow.Api`)**
* **C# / .NET 8 (Minimal APIs):** Construção de uma API moderna, tipada e de alta performance.
* **Entity Framework Core:** ORM para abstração, mapeamento de entidades e persistência de dados.
* **SQLite:** Banco de dados relacional portátil, garantindo execução local imediata sem necessidade de infraestrutura pesada.

**Front-end (`/CashFlow.Web`)**
* **React + TypeScript (via Vite):** Interface SPA (Single Page Application) com forte tipagem estática e componentização inteligente.
* **Axios:** Cliente HTTP para comunicação padronizada com a API.
* **React Toastify:** Notificações não obstrutivas para feedback visual das operações de rede.

---

🧠 Decisões de Arquitetura

1. **Camada de Serviço e Regras de Negócio:** A restrição que impede menores de 18 anos de registrarem "Receitas" foi encapsulada rigidamente no `TransacaoService` do Back-end. A API recusa a transação enviando um `400 Bad Request`. O Front-end apenas intercepta a exceção e notifica o usuário, garantindo a integridade dos dados na origem.
2. **Cascade Delete Nativo:** Utilizando os recursos relacionais do Entity Framework, a funcionalidade de exclusão de usuários engatilha automaticamente a remoção de todas as transações associadas, prevenindo dados órfãos.
3. **Padrão de Features no Front-end:** O projeto web foi estruturado pelo princípio de domínios (`Pessoas` e `Transacoes`), evitando componentes monolíticos e facilitando a manutenibilidade a longo prazo.
4. **Developer Experience (Auto-Migration):** A API está programada para executar as *migrations* pendentes e gerar o arquivo de banco de dados automaticamente na inicialização, oferecendo um ambiente *plug-and-play* para avaliação.

---

 ## 🚀 Como Executar o Projeto

Certifique-se de possuir o **Node.js** e o **.NET 8 SDK** instalados.

## Inicializando a API (Back-end)

1. Navegue até o diretório do servidor:
cd CashFlow.Api

2. Execute a aplicação:
dotnet run

O servidor estará operacional e o banco SQLite será gerado automaticamente.

## Inicializando a Interface (Front-end)

1. Em um novo terminal, acesse o diretório do cliente:
cd CashFlow.Web

2. Instale as dependências:
npm install

3. Inicie o servidor de desenvolvimento:
npm run dev

Acesse o endereço local disponibilizado no terminal (geralmente http://localhost:5173).

⌨️ Desenvolvido por João Victor Varela dos Santos (Estudante de Sistemas para Internet)
