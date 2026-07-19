# Migração para ASP.NET Core

## Estado atual

A solução agora contém duas aplicações:

- `ReciclaFacil`: aplicação MVC 5 original, mantida como referência funcional.
- `ReciclaFacil.Core`: nova aplicação ASP.NET Core em .NET 10, compilável e executável.
- `ReciclaFacil.Api`: API REST versionada para web, Android e iOS.
- `ReciclaFacil.Web`: frontend React/TypeScript independente, sem acesso direto ao banco.

O código moderno está separado em `Domain`, `Application`, `Infrastructure`, `Api` e no
frontend Razor temporário. A solução `ReciclaFacil.Modern.slnx` compila apenas os projetos
modernos e não depende dos targets antigos do Visual Studio.

A experiência pública e a pesquisa de cooperativas já foram portadas para controllers,
Razor Views, injeção de dependência, arquivos estáticos e health check do ASP.NET Core.
O login já lê as tabelas `Usuarios` e `UsuarioRole` por EF Core e mantém compatibilidade
com os hashes de senha do ASP.NET Identity 2. A pesquisa de cooperativas consulta o banco
real, incluindo coordenadas espaciais e materiais comercializados. Quando o LocalDB está
indisponível, dados demonstrativos são usados somente com `ModoMigracao` habilitado.
O módulo administrativo de materiais também foi portado por completo, com operações
assíncronas, autorização por papel, antiforgery e proteção contra exclusão de itens em uso.
A área de cliente já possui painel de coletas, extrato da carteira, notificações e contador
de itens não lidos em modo somente leitura. O schema operacional correspondente é criado
pelo script `Database/002_client_operations_schema.sql`.
O agendamento e o cancelamento de coletas também foram portados com validação de
disponibilidade, materiais aceitos, propriedade do cliente e transações atômicas.
Os cadastros públicos de cooperativa e cliente criam usuário, papel e perfil na mesma
transação. As senhas usam o formato Identity v2 para manter interoperabilidade durante
a convivência com o projeto legado. Os papéis de referência são aplicados pelo script
`Database/003_reference_roles.sql`.

A API agora também autentica os usuários legados por `POST /api/v1/auth/login`, emite
JWTs para web e aplicativos móveis e oferece rotação e revogação de refresh tokens.
Somente o hash SHA-256 do refresh token é persistido. A tabela correspondente é criada
de forma idempotente por `Database/004_refresh_tokens.sql`.

A primeira fatia do novo frontend já consome `GET /api/v1/cooperatives` e
`GET /api/v1/cooperatives/{id}`. Ela oferece pesquisa paginada, filtros, estados de
carregamento/erro/vazio, layout responsivo e detalhes dos materiais comercializados.

O login também foi migrado para o React. A API mantém os contratos de token para
aplicativos móveis e oferece um fluxo web separado que protege o refresh token em cookie
`HttpOnly` com rotação. O frontend mantém o access token somente em memória, restaura a
sessão ao recarregar e revoga o token no logout.

## Próximas etapas

1. Ampliar os endpoints autenticados dos módulos Cliente e Cooperativa.
2. Migrar os painéis de Cliente e Cooperativa para o frontend TypeScript.
3. Portar recuperação de senha e segundo fator para o fluxo da API.
4. Automatizar testes de integração da autenticação e das regras operacionais.
5. Preparar backup, observabilidade e configuração segura para homologação.

## Execução

```powershell
dotnet run --project .\ReciclaFacil.Core\ReciclaFacil.Core.csproj
```

A aplicação responde em `http://localhost:5080` e o health check em `/health`.

## Banco de desenvolvimento

O banco vazio `ReciclaFacilWeb` é criado na instância `.\SQLEXPRESS` pelo script
`Database/001_initial_core_schema.sql`. O script é idempotente e pode ser executado
novamente sem apagar tabelas ou dados.
