# Documentação Final — API Events (Guia Completo de Setup, Testes e Publicação)

> Documento final consolidado para execução ponta a ponta do projeto.
> Objetivo: sair de ambiente zerado até validação funcional completa no Swagger + evidências no banco/disco + publicação no GitHub.

---

## 1) Visão geral do projeto

### 1.1 Objetivo funcional
API de eventos com foco em:
- Listagem de eventos e detalhes.
- Inscrição de projetos por escola (`EnrollmentID`).
- Upload de arquivos por inscrição com regras de negócio.
- Permissões por perfil (`school`, `coordinator`, `professor`).

### 1.2 Arquitetura (camadas)
- `src/01-Presentation`: Controllers, autenticação JWT, Swagger.
- `src/02-Application`: Serviços, DTOs e regras de negócio.
- `src/03-Domain`: Entidades e enums (`EventStatus`, `FileType`).
- `src/04-Infra/4.2-Data`: EF Core, DbContext, migrações, repositórios.
- `src/04-Infra/4.1-CrossCutting`: DI/IoC e `LocalFileStorageService`.

### 1.3 Regras principais implementadas
- `EnrollmentID` é o `Id` de `event_enrollments` (não é `EventId`).
- Soft delete em entidades (`DeleteDate`).
- Limites de upload por inscrição:
  - Máximo total: 6
  - Imagens: 4
  - PDF: 1
  - Vídeo: 1
- Tipos de upload permitidos:
  - `image/jpeg`, `image/png`, `image/gif`, `image/webp`, `application/pdf`, `video/mp4`, `video/webm`, `video/quicktime`

---

## 2) Pré-requisitos e ambiente

## 2.1 Ferramentas
- Docker Desktop com engine ativo.
- .NET SDK 9.
- `dotnet-ef` global tool.
- DBeaver.
- PowerShell (compatível com scripts do projeto).

## 2.2 Diretório de trabalho
Executar comandos em:
`c:\Users\rurik.dangelo2\source\repos\apieventsr\apieventsr`

## 2.3 Banco (docker-compose)
`docker-compose.yml`:
- Container: `apieventsr-db`
- DB: `apieventsr_dev`
- User: `postgres`
- Password: `postgres`
- Porta: `5432`

---

## 3) Setup completo do zero (ordem obrigatória)

## 3.1 Criar appsettings local
```powershell
Copy-Item src\01-Presentation\appsettings.Development.example.json `
          src\01-Presentation\appsettings.Development.json
```

## 3.2 Subir Postgres
```powershell
docker compose up -d
docker ps
```
Esperado: container `apieventsr-db` com status `Up`.

## 3.3 Aplicar migração
```powershell
dotnet ef database update --project src/04-Infra/4.2-Data --startup-project src/01-Presentation
```

## 3.4 Criar pasta de upload
```powershell
New-Item -ItemType Directory -Force -Path C:\uploads\apieventsr | Out-Null
```

## 3.5 Carregar seed
```powershell
Get-Content ..\Tarefas\seed_dados_minimos_evento.sql | docker exec -i apieventsr-db psql -U postgres -d apieventsr_dev
```

## 3.6 Subir API
```powershell
dotnet run --project src/01-Presentation
```
Esperado no console:
- `Now listening on: https://localhost:7164`
- `Now listening on: http://localhost:5074`
- `Hosting environment: Development`

---

## 4) DBeaver — validação de tabelas (ponto crítico)

## 4.1 Conexão correta
- Host: `localhost`
- Port: `5432`
- Database: `apieventsr_dev`
- User: `postgres`
- Password: `postgres`
- Schema: `public`

## 4.2 SQL para confirmar estrutura
```sql
SELECT tablename
FROM pg_tables
WHERE schemaname='public'
ORDER BY tablename;
```
Esperado:
- `DomainEntity`
- `__EFMigrationsHistory`
- `categories`
- `enrollment_files`
- `event_documents`
- `event_enrollments`
- `events`
- `segments`
- `user_segments`

## 4.3 SQL para confirmar migration
```sql
SELECT "MigrationId"
FROM "__EFMigrationsHistory"
ORDER BY "MigrationId";
```
Esperado:
- `20260410120333_InitEventModule`

## 4.4 Se ainda não aparecer no DBeaver
1. Confirmar que está no banco `apieventsr_dev` (não `postgres` padrão).
2. Expanda manualmente `Schemas > public > Tables`.
3. Clique com botão direito em conexão/schema e use `Refresh`.
4. Verifique filtros de objetos ativos no DBeaver.
5. Rode os SQLs acima no SQL Editor do próprio DBeaver.
6. Se SQL listar tabela e árvore não mostrar: é cache/filtro de UI, não ausência física.

---

## 5) Autenticação para Swagger (JWT local)

Em Development, a API aceita JWT HS256 com secret:
`dev-secret-key-only-for-local-testing`

Payload para testes:
```json
{
  "sub": "22222222-2222-2222-2222-222222222222",
  "school_id": "11111111-1111-1111-1111-111111111111",
  "role": "school",
  "exp": 9999999999
}
```

No Swagger:
1. Abrir `https://localhost:7164/swagger`
2. `Authorize`
3. Informar `Bearer <TOKEN>`

Observação: o controller de inscrição aceita `sub` e também `NameIdentifier` (compatibilidade de mapeamento de claims no .NET).

---

## 6) Endpoints e parâmetros de teste (com esperado)

Todos exigem `Authorization: Bearer <token>`.

## 6.1 Eventos

### GET `/api/v1/event/all`
- Path params: nenhum
- Body: nenhum
- Esperado `200`: array de `EventListItemResponse`:
  - `id`, `title`, `bannerUrl`, `enrollmentStartDate`, `enrollmentEndDate`, `status`, `hasEnrollment`, `isCurrent`
- `status` enum:
  - `0` ComingSoon
  - `1` EnrollmentOpen
  - `2` InProgress
  - `3` Closed

### GET `/api/v1/event/{id}`
- Path params:
  - `id` (Guid do evento)
- Esperado `200`: `EventDetailResponse`:
  - `id`, `title`, `description`, `bannerUrl`, `status`, `hasEnrollment`, `documents[]`, `schedule[]`, `awardDetails`, `canEnroll`, `canViewProjects`
- Esperado `404`: evento inexistente

## 6.2 Inscrições

### GET `/api/v1/event-enrollment/segment/{eventId}`
- Path params:
  - `eventId` (Guid)
- Esperado `200`: lista de segmentos com categorias e `isAvailable`

### POST `/api/v1/event-enrollment`
- Body (`CreateEnrollmentRequest`):
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
- Campos obrigatórios:
  - `eventId`, `segmentId`, `categoryId`, `projectName`, `responsibleName`, `managementRepresentative`
- Regras:
  - Evento deve existir e estar com inscrição aberta
  - Segmento/categoria ativos e relacionados
  - Não pode duplicar combinação escola+evento+segmento+categoria
- Esperado `201`: `EnrollmentCreatedResponse`
  - `id` (este é o `EnrollmentID`)
  - `projectName`, `segmentName`, `categoryName`, `createdAt`
- Erros esperados:
  - `404` evento/segmento/categoria inválidos
  - `422` regra de negócio
  - `401` claim inválida (`school_id` ou `sub`)

### GET `/api/v1/event-enrollment/all`
- Esperado `200`: lista de `EnrollmentListItemResponse`
  - `id`, `projectName`, `eventTitle`, `segmentName`, `categoryName`, `eventStatus`, `createdAt`, `canEdit`, `canDelete`, `canUpload`

### PUT `/api/v1/event-enrollment/{id}`
- Path params:
  - `id` (EnrollmentID)
- Body (`UpdateEnrollmentRequest`):
```json
{
  "projectName": "Projeto Demo API - Atualizado",
  "responsibleName": "Responsavel Demo",
  "managementRepresentative": "Representante Demo"
}
```
- Esperado `200`
- Regras:
  - Não altera `eventId`, `segmentId`, `categoryId`
  - Bloqueia edição se evento encerrado
  - Respeita permissão por perfil

### DELETE `/api/v1/event-enrollment/{id}`
- Path params:
  - `id` (EnrollmentID)
- Esperado `204`
- Comportamento: exclusão lógica (`DeleteDate`)

## 6.3 Arquivos de inscrição

### POST `/api/v1/event-enrollment/{enrollmentId}/files`
- Path params:
  - `enrollmentId` (Guid)
- Body: `multipart/form-data` com campo `file`
- Esperado `201` (`UploadFileResponse`):
  - `id`, `originalName`, `blobName`, `contentType`, `fileType`, `sizeInBytes`, `uploadedAt`
- Regras:
  - Inscrição existe e pertence à escola do token
  - Evento não encerrado
  - Tipo MIME permitido
  - Limites por tipo e total
  - Nome original não pode repetir na mesma inscrição

### DELETE `/api/v1/event-enrollment/{enrollmentId}/files/{fileId}`
- Esperado `204`
- Comportamento: remove físico + soft delete no banco

---

## 7) Matriz de cenários de teste (funcional + negativo)

## 7.1 Fluxo feliz principal
1. `GET /event/all` retorna evento com `status = 1`.
2. `GET /event-enrollment/segment/{eventId}` retorna opções `isAvailable=true`.
3. `POST /event-enrollment` retorna `201` e `id`.
4. `POST /event-enrollment/{id}/files` com `.png` retorna `201`.
5. `GET /event-enrollment/all` lista a inscrição criada.

## 7.2 Cenários negativos obrigatórios
1. Token sem `school_id` -> `401`.
2. `POST /event-enrollment` em evento fora da janela -> `422`.
3. `POST /event-enrollment` duplicado (mesma combinação) -> `422`.
4. Upload com `text/plain` -> `422`.
5. Upload 2º PDF na mesma inscrição -> `422`.
6. Upload em inscrição de outra escola -> `422`.
7. Delete de arquivo com `fileId` inexistente -> `404`.

---

## 8) Banco: consultas de evidência

```sql
-- Últimas inscrições
SELECT "Id", "ProjectName", "SchoolId", "EventId", "CreateDate", "DeleteDate"
FROM event_enrollments
ORDER BY "CreateDate" DESC
LIMIT 20;

-- Últimos arquivos
SELECT "Id", "OriginalName", "EventEnrollmentId", "ContentType", "FileType", "SizeInBytes", "CreateDate", "DeleteDate"
FROM enrollment_files
ORDER BY "CreateDate" DESC
LIMIT 20;

-- Ver se soft delete foi aplicado
SELECT COUNT(*)
FROM event_enrollments
WHERE "DeleteDate" IS NOT NULL;
```

---

## 9) Evidências já obtidas nesta execução

Fluxo automatizado validado com sucesso:
- `EnrollmentID`: `b2d25942-455e-42ec-b270-2e0e4eb55164`
- `FileID`: `728db93f-6e3e-462b-8752-5792b3c265c7`
- Arquivo em disco: `C:\uploads\apieventsr\b2d25942-455e-42ec-b270-2e0e4eb55164\1777479387_demo-upload-20260429131627.png`

Validação adicional de upload:
- `EnrollmentID`: `9ede049a-5cce-4a59-9c9f-90f46d220894`
- `FileID`: `c525aef6-733d-475f-abcb-b50281fa7ea2`

---

## 10) Scripts oficiais para teste

## 10.1 Script automatizado principal
Arquivo:
`Tarefas/executar_fluxo_demo_eventos.ps1`

O script:
- Gera JWT local HS256.
- Chama API via HTTPS.
- Busca evento com inscrição aberta.
- Cria inscrição e captura `EnrollmentID`.
- Faz upload de PNG mínimo.
- Mostra evidência no storage.

Execução:
```powershell
powershell -ExecutionPolicy Bypass -File ..\Tarefas\executar_fluxo_demo_eventos.ps1
```

---

## 11) Troubleshooting rápido (mais comuns)

1. **Não vejo tabelas no DBeaver**
   - Verificar DB `apieventsr_dev`.
   - Rodar SQL de `pg_tables`.
   - Refresh/filtro/schema `public`.

2. **`relation does not exist`**
   - Migration não aplicada.
   - Rodar `dotnet ef database update ...`.

3. **`401 Unauthorized`**
   - Token inválido/expirado.
   - Claims ausentes (`school_id`, `sub`).
   - Ambiente não está `Development`.

4. **Upload falha com tipo inválido**
   - Ajustar MIME para lista permitida.

5. **Sem evento aberto para inscrição**
   - Conferir seed e datas do evento.

---

## 12) Publicação no GitHub (passo a passo)

> Este roteiro cobre o envio final para repositório remoto.

## 12.1 Conferir alterações
```powershell
git status
```

## 12.2 Criar branch de entrega
```powershell
git checkout -b docs/final-guia-testes-eventos
```

## 12.3 Adicionar arquivos
```powershell
git add Tarefas/DOCUMENTACAO_FINAL_PROJETO.md
git add Tarefas/GUIA_EXECUCAO.md
git add Tarefas/checklist_cards_demo_eventos.md
git add Tarefas/executar_fluxo_demo_eventos.ps1
git add src/01-Presentation/Controllers/EventEnrollmentController.cs
```

## 12.4 Commit
```powershell
git commit -m "docs: guia final completo de setup e testes + estabilizacao do fluxo demo"
```

## 12.5 Push
```powershell
git push -u origin docs/final-guia-testes-eventos
```

## 12.6 Pull Request
Título sugerido:
`[Eventos] Documentação final de setup/testes + correções de fluxo demo local`

Descrição mínima:
- Contexto e objetivo.
- Alterações realizadas.
- Evidências (EnrollmentID/FileID e prints do DBeaver/Swagger).
- Checklist de validação executada.

---

## 13) Checklist final de aceite (marcar tudo)

- [ ] Docker/Postgres em `Up`.
- [ ] Migration aplicada (`__EFMigrationsHistory`).
- [ ] Tabelas visíveis no DBeaver (schema `public`).
- [ ] Seed carregado.
- [ ] API no ar com Swagger acessível.
- [ ] JWT autorizado no Swagger.
- [ ] Inscrição criada com `EnrollmentID`.
- [ ] Upload realizado com `FileID`.
- [ ] Evidência no disco em `C:\uploads\apieventsr\<EnrollmentID>`.
- [ ] Evidência no banco (`event_enrollments`, `enrollment_files`).
- [ ] Branch enviada ao GitHub com commit e PR.

---

## 14) Arquivos de referência

- `Tarefas/REVISAO_PROJETO.md`
- `Tarefas/checklist_cards_demo_eventos.md`
- `Tarefas/GUIA_EXECUCAO.md`
- `Tarefas/executar_fluxo_demo_eventos.ps1`
- `src/01-Presentation/Controllers/EventController.cs`
- `src/01-Presentation/Controllers/EventEnrollmentController.cs`
- `src/01-Presentation/Controllers/EnrollmentFileController.cs`
- `src/02-Application/Services/EventService.cs`
- `src/02-Application/Services/EventEnrollmentService.cs`
- `src/02-Application/Services/EnrollmentFileService.cs`
- `src/04-Infra/4.2-Data/Migrations/20260410120333_InitEventModule.cs`
