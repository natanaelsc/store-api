# Store API

API RESTful desenvolvida em .NET 8 para gerenciamento de pedidos de e-commerce, construída com **Clean Architecture**.

---

## Arquitetura

O projeto segue os princípios de **Clean Architecture**, com a separação de responsabilidades entre camadas:

```
Store.sln
├── src/
│   ├── Store.Domain          # Entidades, regras de negócio, interfaces
│   ├── Store.Application     # Casos de uso, DTOs, validações, serviços
│   ├── Store.Infrastructure  # EF Core, repositórios, banco de dados
│   └── Store.API             # Controllers, middleware, configuração
└── tests/
    └── Store.UnitTests       # Testes unitários de domínio e aplicação
```

### Fluxo de dependências

```
API → Application → Domain
Infrastructure → Domain
```

A camada de Domínio não conhece nenhuma outra camada — pura lógica de negócio.

---

## Como rodar

### Pré-requisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker + Docker Compose](https://www.docker.com/products/docker-desktop/)

### Com Docker (recomendado)

```bash
# Sobe API + SQL Server
docker compose up --build
```

A API estará disponível em `http://localhost:8080`.

### Localmente

1. Suba um SQL Server (ou ajuste a connection string):

```bash
docker run -e ACCEPT_EULA=Y -e SA_PASSWORD=YourStrong@Passw0rd \
  -p 1433:1433 mcr.microsoft.com/mssql/server:2022-latest
```

1. Ajuste `appsettings.Development.json` com sua connection string.

2. Execute:

```bash
cd src/Store.API
dotnet run
```

As migrations são aplicadas automaticamente.

---

## Documentação da API

| Interface | URL |
|-----------|-----|
| **Scalar** (moderno) | <http://localhost:8080/scalar> |
| **Swagger UI** | <http://localhost:8080/swagger> |
| **OpenAPI JSON** | <http://localhost:8080/openapi/v1.json> |
| **Health Check** | <http://localhost:8080/health> |

---

## Endpoints

Todos os endpoints são versionados com prefixo `/api/v1/`.

### Pedidos

| Método | Rota | Descrição |
|--------|------|-----------|
| `POST` | `/api/v1/orders` | Criar pedido |
| `GET` | `/api/v1/orders` | Listar pedidos (com filtros e paginação) |
| `GET` | `/api/v1/orders/{id}` | Buscar pedido por ID |
| `PUT` | `/api/v1/orders/{id}` | Atualizar itens do pedido |
| `DELETE` | `/api/v1/orders/{id}` | Excluir pedido |
| `PATCH` | `/api/v1/orders/{id}/process` | Processar pedido |
| `PATCH` | `/api/v1/orders/{id}/ship` | Enviar pedido |
| `PATCH` | `/api/v1/orders/{id}/cancel` | Cancelar pedido |

### Filtros de listagem

```http
GET /api/v1/orders?status=Initiated&buyerId={guid}&page=1&pageSize=10
```

### Produtos

| Método | Rota | Descrição |
|--------|------|-----------|
| `POST` | `/api/v1/products` | Criar produto |
| `GET` | `/api/v1/products/{id}` | Buscar produto |

---

## Regras de Transição de Status

```
Initiated ──► Processed ──► Shipped
    │               │
    └───────────────┴──► Cancelled
```

| Transição | Permitida quando |
|-----------|-----------------|
| Initiated → Processed | Sempre que `Initiated` |
| Processed → Shipped | Sempre que `Processed` |
| Initiated → Cancelled | ✅ |
| Processed → Cancelled | ✅ |
| Shipped → Cancelled | ❌ |
| Qualquer alteração de itens | Apenas `Initiated` |

---

## Testes

```bash
dotnet test
```

Cobertura:

- Testes unitários do **Domínio** (Order, OrderItem, Buyer, Product)
- Testes unitários da **Application** (OrderService com NSubstitute)

---

## Diferenciais implementados

| Diferencial | Status |
|-------------|--------|
| Filtro na listagem (status, buyerId, paginação) | ✅ |
| Versionamento de endpoints (`/api/v1/`) | ✅ |
| Scalar UI | ✅ |
| Swagger UI | ✅ |
| Clean Architecture | ✅ |
| Docker + Docker Compose | ✅ |
| Testes unitários (xUnit + NSubstitute + FluentAssertions) | ✅ |
| Health check (`/health`) | ✅ |
| Serilog (observabilidade) | ✅ |
| FluentValidation | ✅ |
| Problem Details (RFC 7807) | ✅ |

---

## Stack

- **.NET 8**
- **Entity Framework Core 8** + SQL Server
- **FluentValidation** — validação de requests
- **Serilog** — logging estruturado
- **Scalar + Swashbuckle** — documentação
- **Asp.Versioning** — versionamento de API
- **xUnit + NSubstitute + FluentAssertions** — testes

---

## Exemplos de uso

### Criar produto

```http
POST /api/v1/products
Content-Type: application/json

{
  "name": "Notebook Dell XPS 15",
  "price": 8999.99
}
```

### Criar pedido

```http
POST /api/v1/orders
Content-Type: application/json

{
  "buyerName": "Maria Silva",
  "buyerEmail": "maria@example.com",
  "items": [
    {
      "productId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
      "quantity": 2
    }
  ]
}
```

### Processar pedido

```http
PATCH /api/v1/orders/{id}/process
```

### Listar com filtros

```http
GET /api/v1/orders?status=Initiated&page=1&pageSize=5
```
