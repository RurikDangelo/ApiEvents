# Documentação Final Completa — API Events

> Guia único de referência para entender o projeto, subir ambiente local, validar banco no DBeaver, testar todos os endpoints disponíveis e apresentar a API ponta a ponta.

---

## 1) O que é este projeto

A API Events é um backend em `.NET 9` para gestão de eventos escolares, com foco em:

- consulta de eventos e detalhes;
- inscrição de projetos por escola;
- upload e remoção de arquivos por inscrição;
- regras de permissão por perfil (`school`, `coordinator`, `professor`);
- persistência em PostgreSQL com EF Core e soft delete.

## 1.1 Escopo funcional atual

O módulo implementa os seguintes blocos:

1. **Eventos**
   - listar eventos com status calculado por data;
   - detalhar evento com documentos, cronograma e premiação.
2. **Inscrições**
   - listar inscrições da escola;
   - consultar segmentos/categorias disponíveis;
   - criar, editar e excluir (soft delete) inscrição.
3. **Arquivos de inscrição**
   - upload com validações de tipo/limite;
   - exclusão lógica + exclusão física do arquivo.

---

## 2) Como o projeto foi feito (arquitetura e implementação)

## 2.1 Arquitetura em camadas

Estrutura principal em `apieventsr/src`:

- `01-Presentation`: Controllers, startup, autenticação e Swagger.
- `02-Application`: serviços, interfaces e DTOs.
- `03-Domain`: entidades e enums.
- `04-Infra/4.2-Data`: DbContext, migrations, repositories.
- `04-Infra/4.1-CrossCutting`: DI, middleware e storage local.

Fluxo padrão da request:

`Controller` -> `Service` -> `Repository` -> `ProjectContext` -> `PostgreSQL`

## 2.2 Implementação por camada

### Presentation (API)

- `EventController`: `/api/v1/event/all`, `/api/v1/event/{id}`.
- `EventEnrollmentController`: CRUD de inscrições + disponibilidade de segmento/categoria.
- `EnrollmentFileController`: upload e delete de arquivo.

### Application (regras de negócio)

- `EventService`
  - calcula `EventStatus` dinamicamente (`ComingSoon`, `EnrollmentOpen`, `InProgress`, `Closed`);
  - define `canEnroll`, `canViewProjects`, `hasEnrollment`;
  - monta cronograma do evento.
- `EventEnrollmentService`
  - valida janela de inscrição;
  - valida segmento/categoria ativos e relacionamento;
  - evita duplicidade ativa de inscrição;
  - aplica permissão por perfil para listar/editar/excluir.
- `EnrollmentFileService`
  - valida MIME type;
  - aplica limites: 6 total / 4 imagens / 1 PDF / 1 vídeo;
  - evita nome original duplicado na mesma inscrição;
  - salva no disco e persiste no banco.

### Domain (modelo)

Entidades principais:

- `Event`
- `EventDocument`
- `EventEnrollment`
- `EnrollmentFile`
- `Segment`
- `Category`
- `UserSegment`

Enums principais:

- `EventStatus`: `ComingSoon`, `EnrollmentOpen`, `InProgress`, `Closed`
- `FileType`: `Image`, `Pdf`, `Video`

### Infra (dados)

- Migration `20260410120333_InitEventModule` cria tabelas, FKs e índices únicos filtrados por `DeleteDate IS NULL`.
- `ExceptionHandler` converte:
  - `KeyNotFoundException` -> `404`
  - `InvalidOperationException` -> `422`
  - demais -> `500`

### Storage local

`LocalFileStorageService` salva arquivos em:

- base: `FILE_STORAGE_PATH` (default local configurado)
- destino: `{FILE_STORAGE_PATH}/{enrollmentId}/{storageName}`

No ambiente local padrão:

- `C:\uploads\apieventsr\{EnrollmentId}\{timestamp_nome}`

---

## 3) Pré-requisitos

- Docker Desktop (rodando)
- .NET SDK 9
- EF Core CLI (`dotnet-ef`)
- DBeaver
- PowerShell

Diretório base para comandos:

`c:\Users\rurik.dangelo2\source\repos\apieventsr\apieventsr`

---

## 4) Como rodar o banco (PostgreSQL)

Arquivo: `apieventsr/docker-compose.yml`

Configuração do container:

- container: `apieventsr-db`
- image: `postgres:16`
- host: `localhost`
- porta: `5432`
- database: `apieventsr_dev`
- user: `postgres`
- password: `postgres`

## 4.1 Subir banco

```powershell
docker compose up -d
docker ps
```

Esperado: container `apieventsr-db` com status `Up`.

## 4.2 Aplicar migration

```powershell
dotnet ef database update --project src/04-Infra/4.2-Data --startup-project src/01-Presentation
```

## 4.3 Carregar seed mínimo

```powershell
Get-Content ..\Tarefas\seed_dados_minimos_evento.sql | docker exec -i apieventsr-db psql -U postgres -d apieventsr_dev
```

Esse seed cria:

- 1 evento aberto para inscrição;
- 2 segmentos;
- 2 categorias;
- vínculos em `user_segments` para o usuário de demo.

---

## 5) Como rodar o backend

## 5.1 Criar appsettings local

```powershell
Copy-Item src\01-Presentation\appsettings.Development.example.json src\01-Presentation\appsettings.Development.json
```

Valores relevantes em `appsettings.Development.example.json`:

- `CONNECTION_STRING`: `Host=localhost;Port=5432;Database=apieventsr_dev;Username=postgres;Password=postgres`
- `AUTHORITY`: `http://localhost:8080/realms/dev`
- `ISSUER`: `http://localhost:8080/realms/dev`
- `FILE_STORAGE_PATH`: `C:\uploads\apieventsr`

## 5.2 Subir API

```powershell
dotnet run --project src/01-Presentation
```

URLs esperadas (`launchSettings.json`):

- `https://localhost:7164`
- `http://localhost:5074`

Swagger:

- `https://localhost:7164/swagger`

Health check:

- `GET https://localhost:7164/api/health-check`

---

## 6) Autenticação local (JWT para testes)

Em `Development`, a API valida token HS256 com secret:

`dev-secret-key-only-for-local-testing`

## 6.1 Claims que a API usa

- `school_id`: obrigatório para inscrições e arquivos.
- `sub` (ou `nameidentifier` no caso do `EventEnrollmentController`): usuário autor.
- `role`: para regras de permissão (`school`, `coordinator`, `professor`).

## 6.2 Payload de exemplo

```json
{
  "sub": "22222222-2222-2222-2222-222222222222",
  "school_id": "11111111-1111-1111-1111-111111111111",
  "role": "school",
  "exp": 9999999999
}
```

## 6.3 Uso no Swagger

1. Abrir `https://localhost:7164/swagger`
2. Botão `Authorize`
3. Informar: `Bearer <TOKEN>`

---

## 7) Todos os endpoints e parâmetros para testar TODOS os pontos da API atual

Base URL local:

`https://localhost:7164/api/v1`

> Todos os endpoints abaixo exigem `Authorization: Bearer <token>`.

## 7.1 Eventos

### 1) Listar eventos

- Método: `GET`
- Rota: `/event/all`
- Body: não possui
- Path params: não possui

Resposta `200` (exemplo):

```json
[
  {
    "id": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    "title": "Evento Demo API",
    "bannerUrl": null,
    "enrollmentStartDate": "2026-05-03T00:00:00Z",
    "enrollmentEndDate": "2026-05-10T00:00:00Z",
    "status": 1,
    "hasEnrollment": false,
    "isCurrent": true
  }
]
```

### 2) Detalhar evento

- Método: `GET`
- Rota: `/event/{id}`
- Path params:
  - `id` (Guid do evento)

Resposta `200` (campos):

- `id`, `title`, `description`, `bannerUrl`
- `status`, `hasEnrollment`, `canEnroll`, `canViewProjects`
- `documents[]`
- `schedule[]`
- `awardDetails`

Erros:

- `404` se evento não existir.

---

## 7.2 Inscrições

### 3) Segmentos/categorias disponíveis no evento

- Método: `GET`
- Rota: `/event-enrollment/segment/{eventId}`
- Path params:
  - `eventId` (Guid)

Resposta `200`:

- lista de segmentos com categorias;
- cada categoria com `isAvailable`.

### 4) Criar inscrição

- Método: `POST`
- Rota: `/event-enrollment`
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

Resposta `201` (`EnrollmentCreatedResponse`):

- `id` (EnrollmentID)
- `projectName`
- `segmentName`
- `categoryName`
- `createdAt`

Erros típicos:

- `401`: claim `school_id`/`sub` ausente ou inválido.
- `404`: evento/segmento/categoria não encontrado.
- `422`: fora da janela de inscrição ou duplicidade ativa.

### 5) Listar inscrições da escola

- Método: `GET`
- Rota: `/event-enrollment/all`

Resposta `200` (itens):

- `id`, `projectName`, `eventTitle`, `segmentName`, `categoryName`
- `eventStatus`, `createdAt`
- `canEdit`, `canDelete`, `canUpload`

### 6) Editar inscrição

- Método: `PUT`
- Rota: `/event-enrollment/{id}`
- Path params:
  - `id` = `EnrollmentID`
- Body (`UpdateEnrollmentRequest`):

```json
{
  "projectName": "Projeto Demo API - Atualizado",
  "responsibleName": "Responsavel Atualizado",
  "managementRepresentative": "Representante Atualizado"
}
```

Resposta `200` com DTO de inscrição.

Erros:

- `404`: inscrição não encontrada.
- `422`: sem permissão, evento encerrado ou regra de perfil.

### 7) Excluir inscrição (soft delete)

- Método: `DELETE`
- Rota: `/event-enrollment/{id}`
- Path params:
  - `id` = `EnrollmentID`

Resposta `204`.

Erros:

- `404`: inscrição não encontrada.
- `422`: sem permissão ou evento encerrado.

---

## 7.3 Arquivos de inscrição

### 8) Upload de arquivo

- Método: `POST`
- Rota: `/event-enrollment/{enrollmentId}/files`
- `Content-Type`: `multipart/form-data`
- Campo obrigatório: `file`

Path params:

- `enrollmentId` = `EnrollmentID`

Tipos MIME permitidos:

- `image/jpeg`
- `image/png`
- `image/gif`
- `image/webp`
- `application/pdf`
- `video/mp4`
- `video/webm`
- `video/quicktime`

Limites:

- até 6 arquivos totais por inscrição;
- até 4 imagens;
- até 1 PDF;
- até 1 vídeo;
- nome original não pode repetir na mesma inscrição.

Resposta `201` (`UploadFileResponse`):

- `id` (`FileID`)
- `originalName`
- `blobName`
- `contentType`
- `fileType`
- `sizeInBytes`
- `uploadedAt`

Erros:

- `404`: inscrição não encontrada.
- `422`: tipo inválido, limite excedido, sem permissão, evento encerrado.

### 9) Excluir arquivo

- Método: `DELETE`
- Rota: `/event-enrollment/{enrollmentId}/files/{fileId}`

Path params:

- `enrollmentId` = `EnrollmentID`
- `fileId` = `FileID`

Resposta `204`.

Erros:

- `404`: arquivo inexistente.
- `422`: arquivo não pertence à inscrição ou sem permissão.

---

## 8) IDs e parâmetros prontos para testes (seed)

Use estes valores do `Tarefas/seed_dados_minimos_evento.sql`:

- `school_id`: `11111111-1111-1111-1111-111111111111`
- `user_id` (`sub`): `22222222-2222-2222-2222-222222222222`
- `eventId`: `aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa`
- `segmentId`:
  - `bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1`
  - `bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2`
- `categoryId`:
  - `cccccccc-cccc-cccc-cccc-ccccccccccc1`
  - `cccccccc-cccc-cccc-cccc-ccccccccccc2`

---

## 9) Como observar tabelas no DBeaver

## 9.1 Conexão correta

- Host: `localhost`
- Port: `5432`
- Database: `apieventsr_dev`
- User: `postgres`
- Password: `postgres`
- Schema: `public`

## 9.2 Caminho na árvore

`Database Navigator -> sua conexão -> Databases -> apieventsr_dev -> Schemas -> public -> Tables`

## 9.3 SQL para validar existência das tabelas

```sql
SELECT tablename
FROM pg_tables
WHERE schemaname = 'public'
ORDER BY tablename;
```

Esperado (mínimo):

- `DomainEntity`
- `__EFMigrationsHistory`
- `categories`
- `enrollment_files`
- `event_documents`
- `event_enrollments`
- `events`
- `segments`
- `user_segments`

## 9.4 SQL para validar migration aplicada

```sql
SELECT "MigrationId"
FROM "__EFMigrationsHistory"
ORDER BY "MigrationId";
```

Esperado:

- `20260410120333_InitEventModule`

## 9.5 SQL para inspecionar dados do fluxo

```sql
SELECT "Id", "ProjectName", "SchoolId", "EventId", "CreateDate", "DeleteDate"
FROM event_enrollments
ORDER BY "CreateDate" DESC
LIMIT 20;

SELECT "Id", "OriginalName", "EventEnrollmentId", "ContentType", "FileType", "SizeInBytes", "CreateDate", "DeleteDate"
FROM enrollment_files
ORDER BY "CreateDate" DESC
LIMIT 20;
```

## 9.6 Troubleshooting DBeaver

1. Confirme que está conectado em `apieventsr_dev` (não `postgres`).
2. Faça `Refresh` na conexão e no schema `public`.
3. Verifique se existe filtro de objetos ativo.
4. Se SQL mostra tabela e árvore não mostra, problema é cache/filtro visual do DBeaver.
5. Se SQL não mostra tabela, migration ainda não foi aplicada.

---

## 10) Como criar cenários de teste da API

## 10.1 Matriz de cenários (funcional + negativo)

### A) Eventos

1. `GET /event/all` com evento aberto -> `status = EnrollmentOpen`.
2. `GET /event/{id}` com ID válido -> `200` com cronograma e docs.
3. `GET /event/{id}` com GUID inexistente -> `404`.

### B) Inscrição

4. `POST /event-enrollment` com payload válido -> `201` e `EnrollmentID`.
5. `POST /event-enrollment` repetindo mesma combinação (evento+segmento+categoria+escola) -> `422`.
6. `POST /event-enrollment` sem `school_id` no token -> `401`.
7. `PUT /event-enrollment/{id}` alterando nomes -> `200`.
8. `DELETE /event-enrollment/{id}` -> `204` e registro com `DeleteDate`.

### C) Arquivo

9. `POST .../files` com PNG -> `201` e `FileID`.
10. `POST .../files` com `text/plain` -> `422`.
11. upload de 2º PDF na mesma inscrição -> `422`.
12. upload de arquivo com mesmo nome original na mesma inscrição -> `422`.
13. `DELETE .../files/{fileId}` -> `204` e `DeleteDate` preenchido no banco.

## 10.2 Estratégia prática para executar testes

1. Subir banco/API.
2. Autorizar no Swagger com token local.
3. Executar fluxo feliz completo (evento -> inscrição -> upload).
4. Executar cenários negativos por endpoint.
5. Registrar evidência:
   - status HTTP;
   - response body;
   - SQL no DBeaver;
   - evidência física em `C:\uploads\apieventsr`.

---

## 11) Roteiro de apresentação da API (como apresentar todos os pontos)

Use a sequência abaixo para uma apresentação de 15-25 minutos.

## 11.1 Abertura (2 min)

- Problema resolvido: gestão de eventos e inscrições com controle por perfil.
- Stack: `.NET 9`, `EF Core`, `PostgreSQL`, `Swagger`, JWT.
- Arquitetura em camadas e foco em regras no `Application`.

## 11.2 Arquitetura e decisões (3-5 min)

- Mostrar separação `Presentation`/`Application`/`Domain`/`Infra`.
- Explicar:
  - status de evento calculado por data;
  - soft delete em entidades;
  - índices únicos para impedir duplicidade ativa;
  - middleware de exceção padronizando `404/422`.

## 11.3 Demonstração ao vivo (8-12 min)

1. `GET /event/all`
2. `GET /event/{id}`
3. `GET /event-enrollment/segment/{eventId}`
4. `POST /event-enrollment` (capturar `EnrollmentID`)
5. `POST /event-enrollment/{enrollmentId}/files` (capturar `FileID`)
6. `GET /event-enrollment/all`
7. `PUT /event-enrollment/{id}`
8. `DELETE /event-enrollment/{id}` ou `DELETE .../files/{fileId}`

## 11.4 Evidências técnicas (3-5 min)

- DBeaver: mostrar tabelas e dados criados.
- SQL de `event_enrollments` e `enrollment_files`.
- Pasta `C:\uploads\apieventsr\{EnrollmentID}` com arquivo salvo.

## 11.5 Encerramento (2 min)

- Pontos fortes: regras críticas centralizadas e validações robustas.
- Próximos incrementos possíveis:
  - endpoint `GET /event-enrollment/{enrollmentId}/files`;
  - validação de duração de vídeo;
  - padronização de nome de arquivo por convenção de negócio.

---

## 12) Checklist final de execução

- [ ] Docker/Postgres em `Up`
- [ ] Migration aplicada (`__EFMigrationsHistory`)
- [ ] Seed carregado
- [ ] API rodando (`https://localhost:7164/swagger`)
- [ ] JWT autorizado no Swagger
- [ ] Fluxo feliz completo executado
- [ ] Cenários negativos principais executados
- [ ] Evidência no DBeaver validada
- [ ] Evidência no disco validada

---

## 13) Arquivos de referência no repositório

- `apieventsr/src/01-Presentation/Controllers/EventController.cs`
- `apieventsr/src/01-Presentation/Controllers/EventEnrollmentController.cs`
- `apieventsr/src/01-Presentation/Controllers/EnrollmentFileController.cs`
- `apieventsr/src/02-Application/Services/EventService.cs`
- `apieventsr/src/02-Application/Services/EventEnrollmentService.cs`
- `apieventsr/src/02-Application/Services/EnrollmentFileService.cs`
- `apieventsr/src/04-Infra/4.2-Data/Migrations/20260410120333_InitEventModule.cs`
- `apieventsr/src/04-Infra/4.1-CrossCutting/ControllerHandler/Handlers/ExceptionHandler.cs`
- `apieventsr/docker-compose.yml`
- `Tarefas/seed_dados_minimos_evento.sql`
