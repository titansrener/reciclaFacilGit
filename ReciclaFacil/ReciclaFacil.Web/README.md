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

Usuários com o papel `Cooperativa` acessam um painel operacional próprio, alimentado por
`/api/v1/cooperatives/me`. Nele é possível criar horários, consultar participantes,
iniciar e finalizar coletas, além de associar materiais e manter seus preços de revenda.
As regras e a autorização ficam na API, permitindo reutilizar os mesmos fluxos em
aplicativos Android e iOS.

O mesmo painel administra a frota e a equipe. Caminhões e funcionários podem ser
cadastrados e vinculados às coletas; cada funcionário recebe uma conta com o papel
`Funcionario`. O esquema correspondente é criado pelo script idempotente
`database/005_cooperative_resources.sql`.

## Verificações

```powershell
npm run lint
npm run build
npm audit
```

Para outro endereço de API, copie `.env.example` para `.env.local` e altere
`VITE_API_URL`. Em produção, o valor recomendado é `/api/v1`, com frontend e API
publicados atrás do mesmo proxy reverso.
