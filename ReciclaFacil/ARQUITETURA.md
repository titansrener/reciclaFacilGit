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

O painel de Cliente obtém seu resumo por `IClientQueries`, implementado em
`Infrastructure`. A API extrai o identificador do JWT e exige o papel `Cliente`; nenhum
identificador de cliente é aceito pela URL, evitando acesso horizontal aos dados de outra
conta.

Agendamento e cancelamento são implementados por `IClientCollectionService`. O serviço
usa transações serializáveis e revalida, no momento da escrita, a cooperativa vinculada,
o horário futuro, o status, a duplicidade e todos os materiais selecionados. Detalhes e
cancelamentos também usam o identificador do JWT, sem aceitar um cliente informado pelo
consumidor.

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
- `POST /auth/web/login`
- `POST /auth/web/refresh`
- `POST /auth/web/logout`
- `GET /clients/me/overview` (papel `Cliente`)
- `GET /clients/me/collection-options` (papel `Cliente`)
- `GET /clients/me/collections/{id}` (papel `Cliente`)
- `POST /clients/me/collections` (papel `Cliente`)
- `DELETE /clients/me/collections/{id}` (papel `Cliente`)
- `GET /health` e `GET /health/ready` (prontidão da API e do SQL Server)
- `GET /health/live` (vivacidade do processo, sem depender do banco)
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

Para implantação, configure a sondagem de vivacidade em `/health/live` e a de
prontidão em `/health/ready`. A rota histórica `/health` também verifica a conexão com
o SQL Server e permanece disponível para compatibilidade.

Para Android e iOS, os endpoints gerais devolvem o par de tokens para armazenamento
seguro do dispositivo. O frontend web usa os endpoints `/auth/web/*`: o refresh token
fica em cookie `HttpOnly`, `SameSite=Strict`, restrito ao caminho da autenticação e nunca
é incluído no JSON. O access token permanece apenas em memória e é restaurado por rotação
do cookie após um recarregamento. As chamadas web também exigem o cabeçalho
`X-ReciclaFacil-Web: 1`.

## Execução

```powershell
dotnet build .\ReciclaFacil.Modern.slnx
dotnet test .\ReciclaFacil.Api.IntegrationTests\ReciclaFacil.Api.IntegrationTests.csproj
dotnet run --project .\ReciclaFacil.Api\ReciclaFacil.Api.csproj
cd .\ReciclaFacil.Web
npm ci
npm run dev
```

Os testes de integração executam a API em memória e validam as fronteiras de
autorização, a validação inicial do login, a proteção específica da sessão web e a
diferença entre vivacidade e prontidão quando o banco está indisponível. Eles não
alteram o banco de desenvolvimento. A cobertura de persistência cria um banco
`ReciclaFacilWebTests_<processo>`, aplica os mesmos scripts versionados, valida cadastro
de cooperativa, duplicidade, papel e login, e elimina esse banco ao final. Essa parte da
suíte requer a instância local `.\SQLEXPRESS`.

## Transição

O frontend Razor continua disponível enquanto suas telas são recriadas no frontend
TypeScript. Novas regras devem entrar em `Application` e `Domain`, nunca em controllers.
O projeto MVC 5 não recebe novas funcionalidades.
