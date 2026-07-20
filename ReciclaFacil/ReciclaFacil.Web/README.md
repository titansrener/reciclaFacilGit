# ReciclaFacil.Web

Frontend independente do Recicla Fácil, criado com React, TypeScript e Vite. A
aplicação não acessa o banco diretamente: todos os dados vêm da API versionada.

## Requisitos

- Node.js 24 LTS ou superior
- `ReciclaFacil.Api` disponível em `http://localhost:5090`

## Desenvolvimento

```powershell
npm ci
npm run dev
```

O frontend responde em `http://localhost:5173`. Durante o desenvolvimento, o
Vite encaminha `/api` para a API local.

## Sessão web

O login usa `/api/v1/auth/web/*`. O refresh token é mantido exclusivamente em cookie
`HttpOnly`, com rotação a cada restauração; o access token fica somente em memória.
Os endpoints que devolvem refresh token no corpo permanecem disponíveis para os
futuros aplicativos Android e iOS, não sendo usados pelo frontend web.

Usuários com o papel `Cliente` são direcionados ao painel carregado por
`GET /api/v1/clients/me/overview`, com resumo de carteira, coletas e notificações.
O mesmo painel permite consultar horários e materiais, agendar, abrir detalhes e cancelar
coletas ainda não iniciadas.

O histórico completo deixou de depender das páginas MVC. Notificações são consultadas
por `GET /api/v1/clients/me/notifications` e movimentações por
`GET /api/v1/clients/me/wallet`, ambos com `page` e `pageSize`. O extrato devolve
também o saldo total, e o histórico de notificações preserva as ações de leitura e de
aceite ou recusa de propostas. A paginação limita cada resposta a no máximo 50 itens,
evitando cargas grandes tanto na web quanto nos futuros aplicativos móveis.

Usuários com o papel `Cooperativa` acessam um painel operacional próprio, alimentado por
`/api/v1/cooperatives/me`. Nele é possível criar horários, consultar participantes,
reagendar horários ainda abertos, iniciar e finalizar coletas, além de associar materiais
e manter seus preços de revenda. O formulário envia o valor de `datetime-local` como
hora local de negócio, evitando deslocamentos de fuso ao persistir no schema legado.
As regras e a autorização ficam na API, permitindo reutilizar os mesmos fluxos em
aplicativos Android e iOS.

O mesmo painel administra a frota e a equipe. Caminhões e funcionários podem ser
cadastrados, editados e vinculados às coletas; cada funcionário recebe uma conta com
o papel `Funcionario`. A edição do funcionário preserva o e-mail usado para entrar e
altera somente nome e nascimento. O esquema correspondente é criado pelo script idempotente
`database/005_cooperative_resources.sql`.

Usuários com o papel `Funcionario` têm um painel próprio em
`/api/v1/employees/me`: consultam apenas as coletas às quais estão atribuídos, veem
clientes e caminhões e registram os pesos recebidos. Para vendedores, a API calcula o
valor a receber com o preço definido pela cooperativa, atualiza todos os estados numa
transação e cria a notificação do cliente. O script
`database/006_purchase_value_precision.sql` preserva os centavos desses valores.

O roteiro operacional usa
`GET /api/v1/employees/me/collections/{id}/route`. A API confirma que o funcionário
está atribuído à coleta, devolve a cooperativa como origem e somente clientes ainda
pendentes como paradas. Pontos com coordenadas são ordenados por proximidade a partir
da posição anterior; endereços sem coordenada continuam disponíveis com um alerta.
O frontend abre direções por URL pública do Google Maps, sem armazenar chave de mapas
no código. O contrato permanece independente do provedor para ser reutilizado pelos
aplicativos Android e iOS.

Quando o cliente vendedor recebe uma proposta, a coleta entra no estado de decisão
pendente (`P`). O painel permite aceitar — concluindo a coleta e criando uma única
movimentação na carteira — ou recusar, devolvendo materiais e atendimento ao funcionário.
As operações são serializáveis, idempotentes por estado e desativam as notificações
respondidas.

Usuários `Admin` acessam um painel exclusivo para o catálogo global de materiais.
Criação, edição e exclusão usam `/api/v1/admin/materials`; descrições duplicadas são
bloqueadas e materiais já utilizados por cooperativas ou coletas não podem ser
apagados.

Visitantes podem criar contas de cliente ou cooperativa sem depender da aplicação
MVC legada. Os formulários usam `/api/v1/auth/register`, validam os dados no servidor
e persistem usuário, papel e perfil em uma única transação. O script idempotente
`database/007_public_registration_constraints.sql` garante a unicidade de CPFs
informados.

O cadastro público também atende empresas, mantendo os campos e a composição de
endereço da solução legada. Contas `Empresa` recebem um painel autenticado de perfil
por meio de `/api/v1/companies/me/overview`. O schema idempotente correspondente está
em `database/008_companies_schema.sql`, e o serviço impede que um mesmo CNPJ seja
usado por empresa e cooperativa.

Clientes podem alterar horário e materiais de uma coleta ainda agendada pelo fluxo
`PUT /api/v1/clients/me/collections/{id}`. A operação revalida disponibilidade,
propriedade e materiais dentro de uma transação serializável; coletas iniciadas,
finalizadas ou vencidas são protegidas contra edição.

## Credenciais

A recuperação e a alteração de senha são oferecidas pela API:

- `POST /api/v1/auth/password/forgot` sempre devolve a mesma resposta, exista ou não
  uma conta para o e-mail informado;
- `POST /api/v1/auth/password/reset` consome um token de uso único;
- `PUT /api/v1/auth/password` exige autenticação e a senha atual.

Os tokens de recuperação expiram em 60 minutos por padrão e somente o hash SHA-256
é persistido pelo script idempotente `database/009_password_reset_tokens.sql`.
Redefinir ou alterar a senha troca o `SecurityStamp` e revoga refresh tokens, encerrando
imediatamente as sessões anteriores.

O envio do link usa as opções `Smtp` e `PasswordReset` da API. Em produção, informe
esses valores por configuração segura, por exemplo `Smtp__Host`, `Smtp__UserName`,
`Smtp__Password` e `PasswordReset__FrontendBaseUrl`. O token só é devolvido no corpo
da resposta quando a API está no ambiente `Development` e
`PasswordReset__ExposeTokenInResponse` está habilitado; esse recurso existe apenas
para desenvolvimento local.

## Verificações

```powershell
npm run lint
npm run build
npm audit
```

Para outro endereço de API, copie `.env.example` para `.env.local` e altere
`VITE_API_URL`. Em produção, o valor recomendado é `/api/v1`, com frontend e API
publicados atrás do mesmo proxy reverso.
