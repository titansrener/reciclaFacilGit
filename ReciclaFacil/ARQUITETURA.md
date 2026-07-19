# Arquitetura moderna

## Projetos

- `ReciclaFacil.Domain`: regras e tipos que não dependem de frameworks.
- `ReciclaFacil.Application`: contratos, casos de uso e DTOs.
- `ReciclaFacil.Infrastructure`: EF Core, SQL Server e implementações dos contratos.
- `ReciclaFacil.Api`: API HTTP versionada que será consumida pela web e pelos aplicativos.
- `ReciclaFacil.Web`: frontend React/TypeScript independente, consumidor da API versionada.
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

O frontend `ReciclaFacil.Web` não referencia `Infrastructure` nem acessa o SQL Server.
Em desenvolvimento, o Vite encaminha `/api` para `http://localhost:5090`; em produção,
frontend e API devem ficar atrás do mesmo proxy reverso.

## API

Base local: `http://localhost:5090/api/v1`

Endpoints iniciais:

- `GET /cooperatives`
- `GET /cooperatives/{id}`
- `GET /materials`
- `POST /auth/login`
- `POST /auth/refresh`
- `POST /auth/revoke`
- `GET /auth/me` (requer bearer token)
- `GET /health`
- `GET /openapi/v1.json`

As pesquisas de cooperativas são paginadas. `pageSize` aceita no máximo 100 registros.

## Autenticação da API

A API emite access tokens JWT de curta duração e refresh tokens rotativos. O refresh
token é devolvido ao cliente uma única vez; no banco fica somente seu hash SHA-256.
Cada renovação revoga o token anterior, e `/auth/revoke` encerra a sessão renovável.

Em desenvolvimento, a chave de assinatura é criada em
`ReciclaFacil.Api/.keys/jwt-signing-key.txt`, diretório ignorado pelo Git. Em produção,
a inicialização exige uma chave externa com pelo menos 32 bytes:

```powershell
$env:Jwt__SigningKey = '<segredo-fornecido-pelo-ambiente>'
```

Aplicativos web, Android e iOS devem guardar refresh tokens em armazenamento seguro e
enviar o access token no cabeçalho `Authorization: Bearer <token>`.

## Execução

```powershell
dotnet build .\ReciclaFacil.Modern.slnx
dotnet run --project .\ReciclaFacil.Api\ReciclaFacil.Api.csproj
cd .\ReciclaFacil.Web
npm ci
npm run dev
```

## Transição

O frontend Razor continua disponível enquanto suas telas são recriadas no frontend
TypeScript. Novas regras devem entrar em `Application` e `Domain`, nunca em controllers.
O projeto MVC 5 não recebe novas funcionalidades.
