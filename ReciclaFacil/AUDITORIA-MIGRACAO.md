# Auditoria de conclusão da modernização

Data da auditoria: 20/07/2026.

## Requisitos e evidências

| Requisito | Estado | Evidência atual |
|---|---|---|
| Preservar `master` | Concluído | `master` permanece em `be419b5d37567db94f04884c73b73712c7947d0d`; todo o trabalho está em `ReciclaFacilWeb`. |
| ASP.NET Core Web API independente | Concluído | `ReciclaFacil.Api` em .NET 10, OpenAPI versionado, autenticação JWT e endpoints por papel. |
| Camadas desacopladas | Concluído | `Domain`, `Application` e `Infrastructure`; a API depende de contratos e não consulta o `DbContext` diretamente. |
| Frontend independente | Concluído | `ReciclaFacil.Web` em React/TypeScript/Vite consome somente `/api/v1`; build de produção e lint aprovados. |
| Banco SQL Server separado | Concluído | `ReciclaFacilWeb` em `.\SQLEXPRESS`, nove scripts idempotentes e executor único validado em duas reaplicações. |
| Reuso por Android e iOS | Concluído como base | Contratos HTTP e tokens bearer não dependem do React; o cookie `HttpOnly` fica restrito às rotas web. |
| Pesquisa pública | Concluído | Filtros, paginação, detalhes e materiais da cooperativa na API e no React. |
| Cadastros públicos | Concluído | Cliente, Cooperativa e Empresa, com usuário/papel/perfil na mesma transação e documentos únicos. |
| Autenticação e credenciais | Concluído | Login móvel/web, refresh rotativo, revogação, logout, recuperação e alteração de senha. |
| Área do Cliente | Concluído | Painel, agendamento/edição/cancelamento, detalhes, notificações, carteira e decisão de proposta. |
| Área da Cooperativa | Concluído | Coletas, reagendamento, materiais, frota, equipe, associações e contatos do cliente. |
| Área do Funcionário | Concluído | Coletas atribuídas, detalhes, roteiro e registro de materiais/pesos. |
| Empresa e Administração | Concluído | Perfil da Empresa e CRUD protegido do catálogo global de materiais. |
| Segurança de exposição | Concluído | Autorização por papel, propriedade via JWT, refresh por hash, cookie seguro, rate limiting e proxies confiáveis. |
| Operação | Concluído | Prontidão com SQL Server, vivacidade independente, backup `COPY_ONLY` com checksum e `RESTORE VERIFYONLY`. |
| Testes automatizados | Concluído para a base migrada | 14 testes: saúde, fronteiras HTTP, rate limiting e cadastro/login/persistência em banco temporário isolado. |
| Separar a solução moderna do Razor/MVC 5 | Concluído | `ReciclaFacil.Modern.slnx` contém somente API, camadas e testes; projetos antigos ficam apenas como referência. |

## Gates executados

```text
dotnet test ReciclaFacil.Api.IntegrationTests\ReciclaFacil.Api.IntegrationTests.csproj --no-restore
Resultado: 14 aprovados, 0 falhas

dotnet build ReciclaFacil.Modern.slnx --no-restore
Resultado: 0 avisos, 0 erros

npm --prefix ReciclaFacil.Web run build
Resultado: build de produção aprovado

npm --prefix ReciclaFacil.Web run lint
Resultado: aprovado
```

O smoke test contra o banco real retornou HTTP 200 em `/health/live`,
`/health/ready`, `/openapi/v1.json` e `/api/v1/cooperatives`, com paginação 1/5.
Após os testes isolados, o banco `master` tinha zero tabelas de usuário e nenhum banco
`ReciclaFacilWebTests_*` permaneceu.

## Limites deliberados

Login externo, SMS e segundo fator do scaffold MVC não eram fluxos operacionais: os
provedores externos estavam comentados e os serviços de envio eram stubs. Uma estratégia
de segundo fator deve ser tratada como funcionalidade nova e desenhada para web e apps.

A publicação em homologação exige valores do ambiente, não código adicional: conexão
SQL, chave JWT, origens CORS, proxy confiável, URL pública e SMTP. Esses segredos e
endereços não devem ser versionados no repositório.

## Conclusão

A migração funcional solicitada está implementada em uma base separada de API, frontend
e banco, preservando o legado como referência e oferecendo contratos reutilizáveis para
futuros aplicativos Android e iOS.
