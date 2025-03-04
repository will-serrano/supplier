### **Resumo da Biblioteca de Contratos (Supplier.Contracts)**

A biblioteca **Supplier.Contracts** fornece **contratos padronizados para mensagens e transações**.

---

### **📌 Principais Tecnologias Utilizadas**
- **.NET 9.0** como framework principal.
- **NuGet Package**: O projeto é empacotado como **biblioteca .dll** para reutilização.
- **Mensageria**: Estrutura padronizada para troca de mensagens (RabbitMQ, Kafka, etc.).
- **Enums e DTOs**: Define formatos padrão para requisições e respostas.

---

### **⚙️ Estrutura do Código**
📂 **Transactions**  
- `MessageWrapper.cs`: Define um **wrapper** para mensagens transacionadas.  
- `TransactionMessageDataConverter.cs`: Converte mensagens entre diferentes formatos.  

📂 **Transactions/Enums**  
- `MessageType.cs`: Enum com tipos de mensagens suportadas.  

📂 **Transactions/Interfaces**  
- `ITransactionMessageData.cs`: Interface base para mensagens de transação.  

📂 **Transactions/Requests**  
- `TransactionRequestMessageData.cs`: Estrutura dos **dados enviados** nas transações.  

📂 **Transactions/Responses**  
- `TransactionResponseMessageData.cs`: Estrutura dos **dados de resposta** das transações.  

📂 **Properties/PublishProfiles**  
- `FolderProfile.pubxml`: Configuração para **publicação** do pacote.  

---

### **🛠️ Funcionalidades Implementadas**
✅ **Contrato Padronizado para Transações**  
- Define **requisições e respostas unificadas** para os serviços.  

✅ **Enumeração de Tipos de Mensagem**  
- Suporta diferentes categorias de mensagens via `MessageType.cs`.  

✅ **Conversão de Mensagens**  
- `TransactionMessageDataConverter.cs` facilita conversões e adaptações de formato.  

✅ **Publicação como Biblioteca NuGet**  
- O código é empacotado como **Supplier.Contracts.dll** para uso por outras APIs.  

---

### **📌 Considerações**
Este projeto serve como **base para comunicação** entre serviços, garantindo **padronização e desacoplamento** entre APIs.

