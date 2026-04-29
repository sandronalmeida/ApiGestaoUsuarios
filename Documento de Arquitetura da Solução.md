# 📄 Documento de Arquitetura da Solução

## 1. Visão Geral
A solução é uma **API RESTful** desenvolvida em **ASP.NET Core**, voltada para gestão de usuários.  
Ela segue princípios de separação de responsabilidades, organização em camadas e boas práticas de desenvolvimento.

---

## 2. Camadas da Arquitetura
- **Apresentação (Controllers)**  
  Responsável por expor endpoints HTTP e retornar respostas.  
  Exemplo: `UsuariosController`.

- **Aplicação/Serviços**  
  Contém a lógica de negócio e orquestra operações entre entidades e repositórios.  
  Exemplo: `UsuarioService`.

- **Domínio**  
  Define entidades e interfaces que representam o núcleo da aplicação.  
  Exemplo: `Usuario` em `Domain.Entities` e `IUsuarioService` em `Domain.Interfaces`.

- **Infraestrutura**  
  Implementa persistência (EF Core), filtros, middlewares, serialização e logging.  
  Exemplo: `ApiGestaoUsuariosDbContext`, `ValidationFilter`, `AuditFilter`.

---

## 3. Cross-Cutting Concerns
- **Filtros Globais**  
  - `ValidationFilter`: valida o estado do modelo antes da execução da action.  
  - `AuditFilter`: registra logs de auditoria em cada requisição.

- **Middlewares Customizados**  
  - `ExceptionHandlingMiddleware`: captura exceções globais e retorna respostas padronizadas.  
  - `ResponseWrapperMiddleware`: mede tempo de resposta e padroniza saída.

- **Serialização JSON**  
  - `SnakeCaseNamingPolicy` para consistência.  
  - Conversor customizado para datas (`DateTimeConverter`).  
  - Ignora valores nulos para manter contrato limpo.

---

## 4. Configuração
- **CORS E Banco de Dados**  
  Política explícita que restringe acesso apenas ao domínio configurado em `appsettings.json`.  
  ```json
  "CorsSettings": {
    "AllowedOrigin": "https://brunotrbr.github.io"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=BD/app.db"
  }
