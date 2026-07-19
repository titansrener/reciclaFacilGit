# Migração para ASP.NET Core

## Estado atual

A solução agora contém duas aplicações:

- `ReciclaFacil`: aplicação MVC 5 original, mantida como referência funcional.
- `ReciclaFacil.Core`: nova aplicação ASP.NET Core em .NET 10, compilável e executável.

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

## Próximas etapas

1. Migrar o `.mdf` para uma instância SQL Server versionada e gerar um backup.
2. Completar o mapeamento EF Core das entidades, chaves compostas e relações N:N.
3. Portar cadastro, recuperação de senha e segundo fator para ASP.NET Core.
5. Portar os módulos Cliente, Cooperativa, Funcionário e Administrador por fatias.
6. Substituir os tipos espaciais antigos por `NetTopologySuite`.
7. Adicionar testes de integração antes de desativar o projeto MVC 5.

## Execução

```powershell
dotnet run --project .\ReciclaFacil.Core\ReciclaFacil.Core.csproj
```

A aplicação responde em `http://localhost:5080` e o health check em `/health`.

## Banco de desenvolvimento

O banco vazio `ReciclaFacilWeb` é criado na instância `.\SQLEXPRESS` pelo script
`Database/001_initial_core_schema.sql`. O script é idempotente e pode ser executado
novamente sem apagar tabelas ou dados.
