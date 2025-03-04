### **Resumo da Solução (Supplier APIs)**

A solução **Supplier APIs** é composta por **quatro microsserviços** que trabalham em conjunto para oferecer um sistema seguro e eficiente de **autenticação, cadastro de clientes e processamento de transações financeiras**.

---

### **📌 Principais Tecnologias Utilizadas**
- **.NET 9.0** como framework principal.
- **Dapper** para acesso rápido ao banco de dados.
- **SQLite** para armazenamento local das informações.
- **JWT (JSON Web Token)** para autenticação segura.
- **RabbitMQ + Rebus** para comunicação assíncrona entre serviços.
- **FluentMigrator** para gerenciamento de banco de dados.
- **Polly** para resiliência em chamadas HTTP.
- **Serilog** para logging estruturado.

---

### **🛠️ Estrutura da Solução**
📂 **Supplier.Auth** (Autenticação)  
- Gerencia **cadastro e login de usuários**.  
- Utiliza **JWT** para autenticação.  
- Armazena usuários e suas permissões.  

📂 **Supplier.Customers** (Clientes)  
- Gerencia **cadastro e consulta de clientes**.  
- Valida **limites de crédito** e bloqueia cadastros inválidos.  
- Expõe endpoints seguros para a API de Transações.  
- Publica eventos no **RabbitMQ** para atualização de limites.  

📂 **Supplier.Transactions** (Transações)  
- Processa **solicitações de transações** dos clientes.  
- Valida limites antes de autorizar operações.  
- Gera **GUID** para cada transação aprovada.  
- Notifica a API de Clientes sobre mudanças no limite via **RabbitMQ**.  
- Implementa **Polly** para resiliência em chamadas à API de Clientes.  

📂 **Supplier.Contracts** (Contratos)  
- Define **modelos padronizados** de comunicação entre serviços.  
- Mantém DTOs, enums e contratos de mensagens.  
- Publicado como biblioteca **NuGet** para reutilização.  

---

### **⚙️ Integração entre os Serviços**
✅ **Autenticação Centralizada**  
- A API de Autenticação gera **tokens JWT**, que são validados pelas APIs de Clientes e Transações.  

✅ **Processamento Seguro de Clientes**  
- Cadastro de clientes é **validado e armazenado** na API de Clientes.  
- Apenas clientes registrados podem realizar transações.  

✅ **Validação e Autorização de Transações**  
- A API de Transações consulta a API de Clientes antes de autorizar qualquer transação.  
- Caso o cliente tenha saldo suficiente, a transação é **aprovada e registrada**.  

✅ **Comunicação Assíncrona via RabbitMQ**  
- A API de Transações publica eventos para atualizar **limites de clientes**.  
- A API de Clientes recebe e processa essas atualizações.  

✅ **Resiliência e Monitoramento**  
- **Polly** implementa retry automático para falhas temporárias em chamadas HTTP.  
- **Serilog** registra eventos importantes e erros operacionais.  

---

### **📌 Considerações**
A solução **Supplier APIs** fornece um ambiente **seguro, modular e escalável**, garantindo:
🔹 **Autenticação JWT para proteção de endpoints**  
🔹 **Validações rigorosas no cadastro de clientes e limites de crédito**  
🔹 **Processamento eficiente de transações com persistência no banco**  
🔹 **Mensageria via RabbitMQ para comunicação rápida e assíncrona**  
🔹 **Resiliência e logging para melhorar estabilidade e monitoramento**  

🚀 **Essa arquitetura permite fácil manutenção e escalabilidade, garantindo um fluxo confiável de autenticação, cadastro e transações.**
