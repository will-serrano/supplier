### **Resumo da API de Autenticação (Supplier.Auth)**

A API de autenticação implementa um sistema de login e registro utilizando **.NET 9.0, Dapper e JWT**, garantindo a segurança dos endpoints de Clientes e Transações.

---

### **📌 Principais Tecnologias Utilizadas**
- **.NET 9.0** como framework principal.
- **Dapper** para acesso rápido ao banco de dados.
- **SQLite** como banco de dados para armazenamento de usuários.
- **JWT (JSON Web Token)** para autenticação segura.
- **FluentMigrator** para controle de migrações do banco.
- **Serilog** para logging da aplicação.

---

### **⚙️ Estrutura do Código**
📂 **Configuration**  
- `DbConnectionFactory.cs`: Configura a conexão com o banco de dados.  
- `JwtSettings.cs`: Configuração do JWT (tempo de expiração, chave secreta).  

📂 **Controllers**  
- `AuthController.cs`: Endpoints para **login e registro de usuários**.  
- `InternalAuthController.cs`: Possivelmente gerencia autenticações internas.  

📂 **DTOs (Data Transfer Objects)**  
- `LoginRequestDto.cs`: Modelo de requisição para login.  
- `RegisterRequestDto.cs`: Modelo para registrar novos usuários.  
- `LoginResponseDto.cs`: Retorno do login, incluindo **JWT Token**.  

📂 **Models**  
- `User.cs`: Modelo da tabela de usuários.  
- `Role.cs`: Modelo para papéis de usuário (Admin, Usuário comum).  

📂 **Repositórios**  
- `UserRepository.cs`: Métodos para manipulação de usuários no banco.  

📂 **Services**  
- `AuthService.cs`: Implementação da lógica de autenticação e registro.  
- `JwtService.cs`: Geração e validação de tokens JWT.  

📂 **Migrations**  
- `CreateAuthTables.cs`: Criação das tabelas de autenticação.  
- `SeedRoles.cs`: Inserção de roles padrão no banco.  

---

### **🛠️ Funcionalidades Implementadas**
✅ **Cadastro de Usuários**  
- Criação de novos usuários com **e-mail e senha**.  
- Senhas devem ser armazenadas de forma segura (hashing).  

✅ **Autenticação JWT**  
- Usuários podem se logar e receber um **Token JWT válido por 1 hora**.  
- O token pode ser usado para acessar outras APIs (Clientes e Transações).  

✅ **Gerenciamento de Papéis (Roles)**  
- Diferenciação entre usuários comuns e administradores.  

✅ **Persistência no Banco (Dapper + SQLite)**  
- Utiliza banco de dados SQLite para facilitar testes locais.  
- Queries otimizadas com Dapper para melhor performance.  

✅ **Migrações Automatizadas**  
- Usa **FluentMigrator** para criar tabelas e popular o banco na inicialização.  

✅ **Logs com Serilog**  
- Registra eventos de login e falhas de autenticação.  
