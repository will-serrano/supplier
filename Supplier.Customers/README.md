### **Resumo da API de Clientes (Supplier.Customers)**

A API **Supplier.Customers** gerencia o cadastro e consulta de clientes, garantindo validações e comunicação com outros serviços.

---

### **📌 Principais Tecnologias Utilizadas**
- **.NET 9.0** como framework principal.
- **Dapper** para acesso rápido ao banco de dados.
- **SQLite** como banco de dados para armazenar os clientes.
- **RabbitMQ + Rebus** para mensageria assíncrona entre serviços.
- **JWT (JSON Web Token)** para autenticação e segurança.
- **FluentValidation** para validação de dados.
- **Serilog** para logging estruturado.

---

### **⚙️ Estrutura do Código**
📂 **Configuration**  
- `DbConnectionFactory.cs`: Configuração da conexão com o banco.  
- `JwtSettings.cs`: Configuração do JWT para autenticação.  
- `DatabaseInitializer.cs`: Inicializa o banco de dados.  

📂 **Controllers**  
- `CustomerController.cs`: Endpoints para **cadastro e consulta de clientes**.  
- `CustomerValidationController.cs`: Valida a existência de clientes.  

📂 **DTOs (Data Transfer Objects)**  
- `CustomerRequestDto.cs`: Modelo para requisição de cadastro.  
- `CustomerResponseDto.cs`: Modelo de resposta com dados do cliente.  
- `ErrorResponseDto.cs`: Modelo para mensagens de erro.  

📂 **Models**  
- `Customer.cs`: Modelo da entidade cliente.  

📂 **Repositories**  
- `CustomerRepository.cs`: Métodos para manipulação dos clientes no banco.  

📂 **Services**  
- `CustomerService.cs`: Implementação da lógica de negócios para clientes.  

📂 **Messaging (RabbitMQ + Rebus)**  
- `CustomerMessageHandler.cs`: Gerencia mensagens recebidas.  
- `RebusMessageSerializer.cs`: Serialização de mensagens enviadas para a fila.  
- `RoutingKeys.cs`: Define chaves de roteamento para RabbitMQ.  

📂 **Migrations**  
- `CreateCustomerTable.cs`: Criação da tabela de clientes.  

📂 **Validators**  
- `CustomerRequestDtoValidator.cs`: Valida os dados antes do cadastro.  

---

### **🛠️ Funcionalidades Implementadas**
✅ **Cadastro e Consulta de Clientes**  
- Registra clientes garantindo regras de validação.  
- Retorna **lista de clientes** com suporte a cache.  

✅ **Validações Rigorosas**  
- Impede cadastro de **clientes duplicados**.  
- Bloqueia clientes com **limite negativo**.  

✅ **Autenticação JWT**  
- Restringe acesso a endpoints sensíveis.  

✅ **Mensageria com RabbitMQ**  
- Publica eventos de atualização de limite dos clientes.  

✅ **Logging com Serilog**  
- Registra eventos de API e erros operacionais.  

---

### **📌 Considerações**
A API de Clientes garante um **cadastro robusto**, comunicação segura e integração via **RabbitMQ** com outros serviços.
