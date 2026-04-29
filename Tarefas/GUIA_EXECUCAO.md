# Guia de Execução — Demo Eventos

> Passo a passo prático para subir o ambiente, validar tabelas no DBeaver, obter um `EnrollmentID` e testar uploads.  
> Todos os comandos devem ser executados a partir da pasta `c:\Users\rurik.dangelo2\source\repos\apieventsr\apieventsr` (a que tem o `docker-compose.yml`), exceto onde indicado.

---

## Pré-requisitos

- Docker Desktop **com Engine rodando** (ícone verde na bandeja).
- .NET 9 SDK (`dotnet --version` ≥ 9.x).
- EF Core CLI: `dotnet tool install --global dotnet-ef` (uma vez).
- DBeaver (para inspecionar o banco).
- Pasta de upload existente: `C:\uploads\apieventsr` (criada no passo 1.4).

---

## Passo 1 — Setup inicial (uma única vez)

### 1.1 Criar `appsettings.Development.json`
Copie o template (o arquivo real está no `.gitignore`):

```powershell
Copy-Item src\01-Presentation\appsettings.Development.example.json `
          src\01-Presentation\appsettings.Development.json
```

### 1.2 Subir o Postgres
```powershell
docker compose up -d
docker ps   # confirmar 'apieventsr-db' Up
```

### 1.3 Aplicar a migration
```powershell
dotnet ef migrations list `
  --project src/04-Infra/4.2-Data `
  --startup-project src/01-Presentation
# Esperado: 20260410120333_InitEventModule (Pending)

dotnet ef database update `
  --project src/04-Infra/4.2-Data `
  --startup-project src/01-Presentation
```

### 1.4 Criar pasta de uploads
```powershell
New-Item -ItemType Directory -Force -Path C:\uploads\apieventsr | Out-Null
```

### 1.5 Carregar dados mínimos (seed)
```powershell
docker exec -i apieventsr-db psql -U postgres -d apieventsr_dev `
  -f - < ..\Tarefas\seed_dados_minimos_evento.sql
```
> Se o redirecionamento não funcionar no PowerShell, alternativa:
> ```powershell
> Get-Content ..\Tarefas\seed_dados_minimos_evento.sql | `
>   docker exec -i apieventsr-db psql -U postgres -d apieventsr_dev
> ```

### 1.6 Abrir o DBeaver para conferir
| Campo | Valor |
|---|---|
| Host | `localhost` |
| Porta | `5432` |
| Database | `apieventsr_dev` |
| Usuário | `postgres` |
| Senha | `postgres` |

Confirme no schema `public`:
- 7 tabelas (`events`, `event_documents`, `segments`, `categories`, `event_enrollments`, `enrollment_files`, `user_segments`)
- `__EFMigrationsHistory` com a linha `20260410120333_InitEventModule`
- `SELECT * FROM events` → 1 linha (`Evento Demo API`)
- `SELECT * FROM segments` → 2 linhas
- `SELECT * FROM categories` → 2 linhas
- `SELECT * FROM user_segments` → 2 linhas

---

## Passo 2 — Subir a API

```powershell
dotnet run --project src/01-Presentation
```
Anote a porta exibida no console (ex.: `https://localhost:7164`). Acesse:
```
https://localhost:<porta>/swagger
```

---

## Passo 3 — Gerar JWT de desenvolvimento

Abra https://jwt.io. Em **HEADER** confirme `"alg": "HS256"`.  
Em **PAYLOAD**:
```json
{
  "sub": "22222222-2222-2222-2222-222222222222",
  "school_id": "11111111-1111-1111-1111-111111111111",
  "role": "school",
  "exp": 9999999999
}
```
Em **VERIFY SIGNATURE** preencha o secret: `dev-secret-key-only-for-local-testing` (deixe o checkbox **base64** desmarcado).

Copie o token do painel da esquerda. No Swagger clique em **Authorize**, cole `Bearer <token>`, **Authorize**, **Close**.

---

## Passo 4 — Roteiro funcional (Swagger)

### 4.1 Listar eventos
`GET /api/v1/event/all` → retorna o "Evento Demo API" com `status: 1` (`EnrollmentOpen`).

### 4.2 Buscar segmentos disponíveis
`GET /api/v1/event-enrollment/segment/{eventId}` com `eventId = aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa` → retorna 2 segmentos com 1 categoria cada.

### 4.3 Criar inscrição (gera o `EnrollmentID`)
`POST /api/v1/event-enrollment`:
```json
{
  "eventId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
  "segmentId": "bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1",
  "categoryId": "cccccccc-cccc-cccc-cccc-ccccccccccc1",
  "projectName": "Projeto Demo API",
  "responsibleName": "Responsavel Demo",
  "managementRepresentative": "Representante Demo"
}
```
Resposta `201` — **anote o `id` retornado, ele é o `EnrollmentID`**.

### 4.4 Upload de imagem
`POST /api/v1/event-enrollment/{enrollmentId}/files` → escolher um `.png` ou `.jpg` → `201`.

### 4.5 Validar evidência
- DBeaver: `SELECT * FROM event_enrollments;` e `SELECT * FROM enrollment_files;`
- Disco: `dir C:\uploads\apieventsr\<EnrollmentID>` mostra o arquivo.

### 4.6 Validar permissões
Troque o token para `role: "professor"` com `sub` diferente:
- `GET /api/v1/event-enrollment/all` → retorna `[]` (não é autor de nada).

---

## Passo 5 — Alternativa automatizada

Em vez do Swagger, rodar o script (já cobre passos 3 e 4):
```powershell
powershell -ExecutionPolicy Bypass -File ..\Tarefas\executar_fluxo_demo_eventos.ps1
```
> O script gera o JWT local (HS256), usa HTTPS (`https://localhost:7164`), lista evento com inscrição aberta, escolhe segmento/categoria disponível, cria a inscrição (captura `EnrollmentID`) e faz upload de um PNG mínimo gerado em runtime.

---

## Comandos úteis do dia a dia

```powershell
# Banco
docker compose stop          # pausar (mantém dados)
docker compose start         # retomar
docker compose down          # remover container (volume preserva dados)
docker compose down -v       # CUIDADO: apaga dados também

# API travada
Get-Process -Name dotnet | Stop-Process -Force
dotnet run --project src/01-Presentation

# Reset completo do banco (perigoso)
docker compose down -v
docker compose up -d
dotnet ef database update --project src/04-Infra/4.2-Data --startup-project src/01-Presentation
Get-Content ..\Tarefas\seed_dados_minimos_evento.sql | docker exec -i apieventsr-db psql -U postgres -d apieventsr_dev
```

---

## Troubleshooting

| Sintoma | Causa provável | Ação |
|---|---|---|
| `connection refused 5432` | Postgres não subiu | `docker ps`; se vazio, `docker compose up -d` |
| `relation "events" does not exist` | Migration não aplicada | `dotnet ef database update ...` |
| `401 Unauthorized` no Swagger | Token ausente/expirado | Gerar novo token em jwt.io e clicar em Authorize |
| `Tipo de arquivo não permitido` | MIME fora da lista | Usar `.png/.jpg/.gif/.webp/.pdf/.mp4/.webm/.mov` |
| Upload retorna `404` | `EnrollmentID` errado | Refazer passo 4.3, copiar o `id` do JSON |
| Docker Desktop com erro 500 no engine | Engine não inicializou | Reiniciar Docker Desktop; aguardar ícone ficar verde |
