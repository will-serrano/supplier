### **Resumo da API de Transações (Supplier.Transactions)**

A API **Supplier.Transactions** gerencia a autorização de transações, validando limites de clientes e registrando operações financeiras.

---

### **📌 Principais Tecnologias Utilizadas**
- **.NET 9.0** como framework principal.
- **Dapper** para acesso eficiente ao banco de dados.
- **SQLite** para armazenamento local das transações.
- **RabbitMQ + Rebus** para comunicação assíncrona com a API de Clientes.
- **JWT (JSON Web Token)** para autenticação e segurança.
- **Polly** para resiliência em chamadas HTTP.
- **Serilog** para logging estruturado.

---

### **⚙️ Estrutura do Código**
📂 **Configuration**  
- `DbConnectionFactory.cs`: Configuração da conexão com o banco.  
- `JwtSettings.cs`: Configuração do JWT para autenticação.  
- `RabbitMqSettings.cs`: Configuração da mensageria.  

📂 **Controllers**  
- `TransactionController.cs`: Endpoint para **autorização de transações**.  

📂 **DTOs (Data Transfer Objects)**  
- `TransactionRequestDto.cs`: Modelo de requisição de transação.  
- `TransactionResponseDto.cs`: Modelo de resposta com status e ID da transação.  

📂 **Enums**  
- `TransactionStatus.cs`: Enum para representar status de transação.  

📂 **Models**  
- `TransactionRequest.cs`: Modelo da entidade de transações.  

📂 **Repositories**  
- `TransactionRequestRepository.cs`: Métodos para manipulação de transações no banco.  

📂 **Services**  
- `TransactionRequestService.cs`: Implementação da lógica de validação e autorização de transações.  

📂 **Messaging (RabbitMQ + Rebus)**  
- `CustomerMessagePublisher.cs`: Publica eventos de atualização de limite.  
- `TransactionMessageHandler.cs`: Gerencia mensagens recebidas.  

📂 **HttpClients**  
- `CustomerValidationClient.cs`: Valida se o cliente existe antes da transação.  

📂 **Migrations**  
- `CreateTransactionRequestTable.cs`: Criação da tabela de transações.  

📂 **Validators**  
- `TransactionRequestDtoValidator.cs`: Valida os dados da requisição antes do processamento.  

---

### **🛠️ Funcionalidades Implementadas**
✅ **Autorização de Transações**  
- Processa transações e valida **limite de crédito do cliente**.  
- Gera um **GUID** para cada transação aprovada.  

✅ **Validação de Cliente**  
- Consulta a **API de Clientes** para verificar se o cliente está cadastrado.  

✅ **Persistência no Banco (Dapper + SQLite)**  
- Registra todas as transações aprovadas.  

✅ **Mensageria com RabbitMQ**  
- Notifica a **API de Clientes** quando uma transação altera o limite disponível.  

✅ **Resiliência com Polly**  
- Implementa **retry automático** para chamadas à API de Clientes.  

✅ **Autenticação JWT**  
- Restringe acesso a endpoints sensíveis.  

✅ **Logging com Serilog**  
- Registra todas as transações e falhas de operação.  

---

### **📌 Considerações**
A API de Transações garante **segurança, eficiência e integração** com a API de Clientes via RabbitMQ.
