# Migração para ASP.NET Core

## Estado atual

A base funcional foi migrada para uma arquitetura independente e reutilizável:

- `ReciclaFacil.Api`: ASP.NET Core Web API versionada em .NET 10;
- `ReciclaFacil.Web`: frontend React/TypeScript, sem acesso direto ao banco;
- `ReciclaFacil.Domain`, `Application` e `Infrastructure`: regras, contratos e EF Core;
- `ReciclaFacilWeb`: banco SQL Server criado por scripts numerados e idempotentes;
- `ReciclaFacil.Api.IntegrationTests`: testes HTTP e de persistência com banco isolado.

`ReciclaFacil.Modern.slnx` não inclui MVC 5 nem o Razor intermediário. Os projetos
`ReciclaFacil` e `ReciclaFacil.Core` permanecem no repositório somente como referência
histórica e funcional; não participam do build nem da implantação moderna.

## Paridade funcional

O frontend React e a API cobrem:

- pesquisa pública paginada e detalhes de cooperativas;
- cadastro de Cliente, Cooperativa e Empresa;
- login web, login para aplicativos, rotação/revogação de refresh token e logout;
- recuperação e alteração de senha com revogação das sessões;
- painel do Cliente, agendamento, edição, cancelamento, notificações, carteira e decisão de proposta;
- painel da Cooperativa, horários, materiais, frota, equipe, associações e dados do cliente atendido;
- painel do Funcionário, coletas atribuídas, roteiro e registro de materiais/pesos;
- painel da Empresa;
- administração do catálogo global de materiais.

Os hashes Identity v2 continuam aceitos para interoperabilidade com as contas legadas.
Os contratos gerais de autenticação e negócio podem ser consumidos por Android e iOS;
somente a sessão web usa cookie `HttpOnly` específico.

As telas scaffoldadas de login externo, SMS e segundo fator não foram consideradas
paridade operacional: os provedores externos estavam comentados e os serviços de e-mail
e SMS do MVC legado não enviavam mensagens. Uma futura estratégia de segundo fator deve
ser definida como produto novo para web e aplicativos móveis.

## Segurança e operação

- JWT de curta duração e refresh tokens rotativos persistidos somente por hash SHA-256;
- autorização por papel e identificação do proprietário extraída do token;
- cookie web `HttpOnly`, `SameSite=Strict` e marcador obrigatório do cliente web;
- limitação por IP em login, recuperação de senha e cadastros públicos;
- proxies encaminhados aceitos somente quando configurados como confiáveis;
- `/health/live` para vivacidade e `/health/ready` para prontidão com SQL Server;
- chave JWT externa obrigatória fora de Development;
- backup `COPY_ONLY` com checksum e `RESTORE VERIFYONLY`.

## Execução

```powershell
dotnet build .\ReciclaFacil.Modern.slnx
dotnet test .\ReciclaFacil.Api.IntegrationTests\ReciclaFacil.Api.IntegrationTests.csproj
dotnet run --project .\ReciclaFacil.Api\ReciclaFacil.Api.csproj

cd .\ReciclaFacil.Web
npm ci
npm run dev
```

A API local usa `http://localhost:5090`; o Vite usa `http://localhost:5173` e encaminha
`/api` para a API. Em produção, os dois devem ficar atrás do mesmo proxy reverso.

## Banco de desenvolvimento

Para criar ou atualizar o `ReciclaFacilWeb` em `.\SQLEXPRESS`:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Database\Apply-Database.ps1
```

O comando localiza o `sqlcmd`, valida a sequência dos nove scripts e para na primeira
falha. Todos os scripts podem ser reaplicados sem apagar dados existentes.

Antes de uma publicação, gere e verifique um backup independente:

```powershell
powershell -NoProfile -ExecutionPolicy Bypass -File .\Database\Backup-Database.ps1
```

O backup usa o diretório padrão da instância e somente é reportado como concluído após
`RESTORE VERIFYONLY` com checksum.

## Validação

A suíte automatizada cobre sondagens, autorização anônima, validações de autenticação,
rate limiting e cadastro/login real de cooperativa. Para persistência, cria um banco
`ReciclaFacilWebTests_<processo>`, aplica os mesmos nove scripts e o remove ao final,
sem alterar o banco de desenvolvimento.

Antes de homologar, ainda é necessário fornecer valores próprios do ambiente para
conexão SQL, chave JWT, origens CORS, proxy confiável, URL pública e SMTP.
