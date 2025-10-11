# FCG.Users.Api

> Microserviço de autenticação e gestão de usuários do FIAP Cloud Games — .NET 8 com Clean Architecture e JWT


##  Sobre o Projeto

A **FCG.Users.Api** é o microserviço de autenticação e autorização da plataforma FIAP Cloud Games. Desenvolvida seguindo **Clean Architecture** e **Domain-Driven Design (DDD)**, oferece:

-  **Cadastro de usuários** com validações de domínio robustas
-  **Autenticação via JWT** com roles (User/Admin)
-  **Hash de senhas** com BCrypt
-  **Event Sourcing** completo (timeline de eventos por usuário)
-  **Promoção automática**: primeiro usuário vira Admin
-  **Observabilidade**: logs estruturados (Serilog) e tracing distribuído (OpenTelemetry → AWS X-Ray)

##  Arquitetura

###  Estrutura do Projeto (Clean Architecture)

```
FCG.Users/
├── src/
│   ├── FCG.Users.Api/              #  Presentation Layer
│   │   ├── Program.cs              # Bootstrap, DI, Endpoints
│   │   └── appsettings.json
│   │
│   ├── FCG.Users.Application/      #  Application Layer
│   │   ├── UseCases/
│   │   │   ├── CreateUser/         # Criar usuário
│   │   │   ├── Login/              # Autenticar e gerar JWT
│   │   │   ├── GetUserById/        # Buscar por ID
│   │   │   ├── GetUserByEmail/     # Buscar por email
│   │   │   ├── ListUsers/          # Listar todos
│   │   │   ├── UpdateUser/         # Atualizar dados
│   │   │   └── DeleteUser/         # Remover usuário
│   │   ├── Events/
│   │   │   └── EventRecord.cs      # DTO de eventos
│   │   ├── Interfaces/
│   │   │   ├── IEventStore.cs
│   │   │   └── IJwtTokenGenerator.cs
│   │   └── Services/
│   │       └── JwtTokenGenerator.cs # Geração de tokens JWT
│   │
│   ├── FCG.Users.Domain/           #  Domain Layer
│   │   ├── Entities/
│   │   │   └── User.cs             # Agregado raiz
│   │   ├── ValueObjects/
│   │   │   ├── Email.cs            # VO com validação regex
│   │   │   ├── Password.cs         # VO com validação complexa
│   │   │   └── Profile.cs          # User | Admin
│   │   ├── Interfaces/
│   │   │   ├── IUserRepository.cs
│   │   │   ├── IUserCreationService.cs
│   │   │   ├── IUserAuthenticationService.cs
│   │   │   ├── IUserValidationService.cs
│   │   │   └── IPasswordHasher.cs
│   │   └── Services/
│   │       ├── UserCreationService.cs
│   │       ├── UserAuthenticationService.cs
│   │       └── UserValidationService.cs
│   │
│   └── FCG.Users.Infra/            #  Infrastructure Layer
│       ├── Data/
│       │   ├── UsersDbContext.cs   # EF Core Context
│       │   └── StoredEvent.cs      # Entidade Event Store
│       ├── Repositories/
│       │   └── MySqlUserRepository.cs
│       ├── Events/
│       │   └── EfEventStore.cs     # Event Sourcing com EF
│       └── Security/
│           └── BCryptPasswordHasher.cs # Implementação BCrypt
│
├── .aws/
│   └── ecs-taskdef.json           # Task Definition ECS
├── .github/
│   └── workflows/
│       ├── ci.yml                 # Pipeline de testes
│       ├── cd.yml                 # Deploy automático
│       └── docker.yml             # Docker Hub
└── tests/
    └── FCG.Users.UnitTests/       # Testes de domínio
```

###  Infraestrutura AWS

| Recurso | Identificador | Descrição |
|---------|---------------|-----------|
| **ECS Cluster** | `fcg-cluster` | Cluster Fargate compartilhado |
| **ECS Service** | `fcg-users-svc` | Service com auto-scaling |
| **Task Definition** | `fcg-users-task` | 256 CPU / 512 MB RAM |
| **Load Balancer** | `alb-fcg-users` | ALB público (porta 80) |
| **Target Group** | `tg-fcg-users` | Health check em `/health` |
| **CloudWatch Logs** | `/ecs/fcg-users` | Logs JSON estruturados |
| **X-Ray Service** | `FCG.Users.Api` | Tracing distribuído |
| **RDS MySQL** | `fcg_users` | Banco de dados principal |

##  Endpoints da API

###  Health Checks

```http
# Health
GET /health
Response: 200 OK { "status": "Healthy" }

# (verifica MySQL)
GET /health/db
Response: 200 OK | 503 Service Unavailable
```

###  Gestão de Usuários

#### Criar usuário
```http
POST /api/users
Content-Type: application/json

{
  "name": "João Silva",
  "email": "joao@example.com",
  "password": "Senha@123"
}

Response: 201 Created
{
  "id": "9a8f7e6d-5c4b-3a2f-1e0d-9c8b7a6f5e4d",
  "name": "João Silva",
  "email": "joao@example.com",
  "profile": "User"
}
```

**Validações implementadas:**
-  **Email**: formato válido (regex), único no sistema, convertido para lowercase
-  **Password**: mínimo 8 caracteres, 1 maiúscula, 1 minúscula, 1 número, 1 caractere especial
-  **Name**: obrigatório, trimmed
-  **Primeiro usuário**: automaticamente promovido a Admin

#### Login (gerar JWT)
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "joao@example.com",
  "password": "Senha@123"
}

Response: 200 OK
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiresAtUtc": "2025-10-10T14:30:00Z"
}
```

**Token JWT contém:**
```json
{
  "sub": "9a8f7e6d-5c4b-3a2f-1e0d-9c8b7a6f5e4d",
  "nameid": "9a8f7e6d-5c4b-3a2f-1e0d-9c8b7a6f5e4d",
  "name": "João Silva",
  "email": "joao@example.com",
  "role": "User",
  "iss": "fcg-users",
  "aud": "fcg-clients",
  "exp": 1728572400
}
```

#### Buscar usuário por ID
```http
GET /api/users/{id}
Authorization: Bearer {token}

Response: 200 OK
{
  "id": "9a8f7e6d-...",
  "name": "João Silva",
  "email": "joao@example.com",
  "profile": "User"
}

Response: 404 Not Found
```

#### Buscar usuário por email
```http
GET /api/users/by-email?email=joao@example.com
Authorization: Bearer {token}

Response: 200 OK
{
  "id": "9a8f7e6d-...",
  "name": "João Silva",
  "email": "joao@example.com",
  "profile": "User"
}
```

#### Listar todos os usuários
```http
GET /api/users
Authorization: Bearer {token}

Response: 200 OK
{
  "users": [
    {
      "id": "9a8f7e6d-...",
      "name": "João Silva",
      "email": "joao@example.com",
      "profile": "User"
    },
    {
      "id": "8b7c6d5e-...",
      "name": "Admin User",
      "email": "admin@example.com",
      "profile": "Admin"
    }
  ]
}
```

#### Atualizar usuário
```http
PUT /api/users/{id}
Authorization: Bearer {token}
Content-Type: application/json

{
  "name": "João da Silva",
  "newPassword": "NovaSenha@456"
}

Response: 200 OK
{
  "id": "9a8f7e6d-...",
  "name": "João da Silva",
  "email": "joao@example.com",
  "profile": "User"
}
```

**Regras:**
- name e newPassword são opcionais
- Se enviar apenas name, atualiza só o nome
- Se enviar newPassword, valida e gera novo hash BCrypt

#### Deletar usuário
```http
DELETE /api/users/{id}
Authorization: Bearer {token}

Response: 200 OK
{
  "deleted": true
}

Response: 404 Not Found
```

###  Event Sourcing

```http
GET /api/users/{id}/events
Authorization: Bearer {token}

Response: 200 OK
[
  {
    "id": "evt-123",
    "aggregateId": "9a8f7e6d-...",
    "type": "UserCreated",
    "data": "{\"id\":\"9a8f7e6d-...\",\"name\":\"João Silva\",\"email\":\"joao@example.com\",\"profile\":\"User\"}",
    "createdAtUtc": "2025-10-10T10:00:00Z"
  },
  {
    "id": "evt-124",
    "aggregateId": "9a8f7e6d-...",
    "type": "UserUpdated",
    "data": "{\"name\":\"João da Silva\"}",
    "createdAtUtc": "2025-10-10T11:30:00Z"
  }
]
```

##  Segurança

### Hash de Senhas (BCrypt)

```csharp
// Criação de usuário
var plainPassword = "Senha@123";
var hashedPassword = _hasher.Hash(plainPassword);
// hashedPassword: "$2a$11$xN3.../..." (60 caracteres)

// Autenticação
var isValid = _hasher.Verify(plainPassword, hashedPassword);
// isValid: true
```

**Características:**
- Algoritmo: BCrypt (salted + adaptive)
- Work factor: 11 (default)
- Resistente a rainbow tables e brute force
- Hash armazenado no campo `password_hash` do MySQL

### Validação de Email

```csharp
// Regex aplicada
^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$

// Exemplos válidos
"joao@example.com"       
"user.name+tag@domain.co.uk"  

// Exemplos inválidos
"invalid.email"          
"@example.com"           
"user@"                  
```

### Validação de Password

```csharp
// Regex aplicada
^(?=.*\p{Lu})(?=.*\p{Ll})(?=.*\d)(?=.*[\p{P}\p{S}]).{8,}$

// Requisitos:
// - Mínimo 8 caracteres
// - 1 letra maiúscula (\p{Lu})
// - 1 letra minúscula (\p{Ll})
// - 1 número (\d)
// - 1 caractere especial ([\p{P}\p{S}])

// Exemplos válidos
"Senha@123"    
"MyP@ssw0rd"   
"Str0ng!Pass"  

// Exemplos inválidos
"senha123"      (falta maiúscula e especial)
"SENHA@123"     (falta minúscula)
"Senha1234"     (falta especial)
"Pass@1"        (menos de 8 caracteres)
```

### JWT: Estrutura e Validação

**Claims incluídas:**
```json
{
  "sub": "userId",           // Subject (ID do usuário)
  "jti": "tokenId",          // JWT ID (único)
  "nameid": "userId",        // ClaimTypes.NameIdentifier
  "name": "João Silva",      // ClaimTypes.Name
  "email": "joao@test.com",  // ClaimTypes.Email
  "role": "User",            // ClaimTypes.Role (User | Admin)
  "iss": "fcg-users",        // Issuer
  "aud": "fcg-clients",      // Audience
  "nbf": 1728565200,         // Not Before
  "exp": 1728572400,         // Expiration (2h padrão)
  "iat": 1728565200          // Issued At
}
```

**Validação em outros serviços:**

```csharp
// Games e Payments APIs usam a mesma chave JWT
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o => {
        o.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true,
            ValidIssuer = "fcg-users",
            ValidateAudience = true,
            ValidAudience = "fcg-clients",
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey)
            ),
            ValidateLifetime = true
        };
    });
```

##  Configuração

### Variáveis de Ambiente

| Variável | Descrição | Produção (AWS) | Local |
|----------|-----------|----------------|-------|
| `ASPNETCORE_URLS` | Endereço de binding | `http://+:8080` | `http://+:8080` |
| `ConnectionStrings__UsersDb` | MySQL connection string | SSM Parameter | `Server=localhost;Port=3307;...` |
| `Jwt__Key` | Chave secreta JWT (min 32 chars) | SSM Parameter | `DEV_ONLY_CHANGE_THIS_...` |
| `Jwt__Issuer` | Emissor do token | `fcg-users` | `fcg-users` |
| `Jwt__Audience` | Audiência do token | `fcg-clients` | `fcg-clients` |
| `Swagger__EnableUI` | Habilitar Swagger UI | `true` | `true` |
| `OTEL_EXPORTER_OTLP_ENDPOINT` | Endpoint OTLP | `http://127.0.0.1:4317` | `http://127.0.0.1:4317` |
| `OTEL_EXPORTER_OTLP_PROTOCOL` | Protocolo OTLP | `grpc` | `grpc` |
| `OTEL_SERVICE_NAME` | Nome no X-Ray | `FCG.Users.Api` | `FCG.Users.Api` |

###  AWS Systems Manager Parameters

```bash
# Connection string MySQL
arn:aws:ssm:us-east-2:536765581095:parameter/fcg/users/ConnectionStrings__UsersDb

# Chave JWT compartilhada (usado por Users, Games e Payments)
arn:aws:ssm:us-east-2:536765581095:parameter/fcg/users/Jwt__Key
```

###  Exemplo de appsettings.Development.json

```json
{
  "ConnectionStrings": {
    "UsersDb": "Server=localhost;Port=3307;Database=fcg_users;User=fcg;Password=fcgpwd;SslMode=None"
  },
  "Jwt": {
    "Issuer": "fcg-users",
    "Audience": "fcg-clients",
    "Key": "DEV_ONLY_CHANGE_THIS_32CHARS_MINIMUM________________"
  },
  "Swagger": {
    "EnableUI": true
  },
  "Serilog": {
    "MinimumLevel": {
      "Default": "Information",
      "Override": {
        "Microsoft": "Warning",
        "System": "Warning"
      }
    },
    "WriteTo": [
      {
        "Name": "Console",
        "Args": {
          "formatter": "Serilog.Formatting.Compact.CompactJsonFormatter, Serilog.Formatting.Compact"
        }
      }
    ]
  }
}
```

##  Desenvolvimento Local

###  Executar com Docker Compose

```bash
# Subir MySQL e API Users
docker compose up -d mysql-users users

# Verificar logs
docker compose logs -f users

# Acessar serviços
# API: http://localhost:8081
# Swagger: http://localhost:8081/swagger
# MySQL: localhost:3307
```

###  Executar localmente

```bash
# 1. Restaurar dependências
dotnet restore

# 2. Aplicar migrations (criar banco)
cd src/FCG.Users.Api
dotnet ef database update --project ../FCG.Users.Infra

# 3. Executar aplicação
dotnet run --project src/FCG.Users.Api

# 4. Acessar Swagger
# http://localhost:5298/swagger
```

###  Executar testes

```bash
# Todos os testes
dotnet test
```

##  Observabilidade

<img width="1910" height="791" alt="Captura de tela 2025-10-10 220013" src="https://github.com/user-attachments/assets/00dbaeb6-c3a0-4de6-923b-f81256af3cd9" />


### Logs Estruturados (Serilog)

```json
{
  "@t": "2025-10-10T10:30:45.123Z",
  "@mt": "User {UserId} created with email {Email}",
  "@l": "Information",
  "UserId": "9a8f7e6d-...",
  "Email": "joao@example.com",
  "ServiceName": "fcg-users",
  "MachineName": "ip-10-0-1-42",
  "ProcessId": 1,
  "ThreadId": 12
}
```

**Visualizar logs:**
```bash
# CloudWatch Logs (AWS)
aws logs tail /ecs/fcg-users --follow --region us-east-2

# Docker local
docker compose logs -f users
```

### Métricas & Alarmes

**CloudWatch Dashboard:**
- CPU e memória do container
- Tempo médio de resposta
- Health

**Alarme de Health:**
```
arn:aws:cloudwatch:us-east-2:536765581095:alarm:FCG-Users-Health
```

## Deploy

### CI/CD Pipelines

#### Pipeline CI (`.github/workflows/ci.yml`)
```yaml
name: CI
on: [pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with: { dotnet-version: '8.0.x' }
      - run: dotnet restore
      - run: dotnet build -c Release
      - run: dotnet test -c Release
```

#### Pipeline CD (`.github/workflows/cd.yml`)
```yaml
name: CD to ECS
on:
  push:
    branches: [main]

jobs:
  deploy:
    steps:
      - name: Build & Push to ECR
      - name: Update Task Definition
      - name: Deploy to ECS
```

### Deploy Manual

```bash
# 1. Build da imagem
docker build -t fcg-users:latest .

# 2. Login no ECR
aws ecr get-login-password --region us-east-2 | \
  docker login --username AWS --password-stdin \
  536765581095.dkr.ecr.us-east-2.amazonaws.com

# 3. Tag e push
docker tag fcg-users:latest \
  536765581095.dkr.ecr.us-east-2.amazonaws.com/fcg-users:latest
docker push 536765581095.dkr.ecr.us-east-2.amazonaws.com/fcg-users:latest

# 4. Force deployment
aws ecs update-service \
  --cluster fcg-cluster \
  --service fcg-users-svc \
  --force-new-deployment \
  --region us-east-2
```

João Melo FIAP
