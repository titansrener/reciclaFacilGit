# Arquitetura moderna

## Projetos

- `ReciclaFacil.Domain`: regras e tipos que não dependem de frameworks.
- `ReciclaFacil.Application`: contratos, casos de uso e DTOs.
- `ReciclaFacil.Infrastructure`: EF Core, SQL Server e implementações dos contratos.
- `ReciclaFacil.Api`: API HTTP versionada que será consumida pela web e pelos aplicativos.
- `ReciclaFacil.Core`: frontend Razor temporário, mantido funcional durante a criação do frontend independente.
- `ReciclaFacil`: aplicação ASP.NET MVC 5 original, somente para referência durante a migração.

As dependências seguem para dentro:

```text
API ───────────────┐
                   ├──> Application ──> Domain
Infrastructure ────┘
```

O projeto `Infrastructure` implementa as interfaces de `Application`. A API não consulta o
`DbContext` diretamente. Android, iOS e o novo frontend web usarão os mesmos endpoints.

## API

Base local: `http://localhost:5090/api/v1`

Endpoints iniciais:

- `GET /cooperatives`
- `GET /cooperatives/{id}`
- `GET /materials`
- `GET /health`
- `GET /openapi/v1.json`

As pesquisas de cooperativas são paginadas. `pageSize` aceita no máximo 100 registros.

## Execução

```powershell
dotnet build .\ReciclaFacil.Modern.slnx
dotnet run --project .\ReciclaFacil.Api\ReciclaFacil.Api.csproj
```

## Transição

O frontend Razor continua disponível enquanto suas telas são recriadas no frontend
TypeScript. Novas regras devem entrar em `Application` e `Domain`, nunca em controllers.
O projeto MVC 5 não recebe novas funcionalidades.
