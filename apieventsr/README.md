# API de Eventos — Poliedro

API backend em **.NET 9** para o módulo de Eventos da plataforma Poliedro.  
Permite que escolas visualizem eventos, inscrevam projetos e gerenciem arquivos.

---

## Sumário

1. [Visão Geral](#visão-geral)
2. [Arquitetura](#arquitetura)
3. [Configuração do Ambiente Local](#configuração-do-ambiente-local)
4. [Cards Atendidos](#cards-atendidos)
5. [O que foi implementado](#o-que-foi-implementado)
6. [Próximos Passos](#próximos-passos)
7. [Como Testar](#como-testar)
8. [Decisões Técnicas](#decisões-técnicas)

---

## Visão Geral

O módulo de Eventos permite:

- Listar eventos com status calculado automaticamente por datas
- Ver o detalhe de um evento (informações gerais, documentos, cronograma, premiação)
- Inscrever projetos em eventos
- Fazer upload de arquivos vinculados à inscrição
- Listar, editar e excluir projetos inscritos (com regras de permissão por perfil)

### Perfis de usuário

| Perfil | O que pode fazer |
|---|---|
| **Escola** | Edita e exclui qualquer projeto da escola |
| **Coordenador** | Edita e exclui apenas projetos dos segmentos com vínculo |
| **Professor** | Edita e exclui apenas o projeto do qual é autor |

---

## Arquitetura

O projeto segue a **Arquitetura Limpa em Camadas**:

```
src/
├── 01-Presentation        ← Controllers, Startup, Program
│   └── Controllers/
│       ├── EventController.cs
│       └── EntityController.cs  (boilerplate)
│
├── 02-Application         ← Regras de negócio, DTOs, Interfaces, Services
│   ├── Dtos/
│   │   ├── Requests/
│   │   └── Responses/
│   ├── Interfaces/
│   ├── Mappers/           ← Perfis do AutoMapper (DTO ↔ Entidade)
│   └── Services/
│
├── 03-Domain              ← Entidades e Enums (sem dependência de nada)
│   ├── Entities/
│   └── Enums/
│
└── 04-Infra/
    ├── 4.1-CrossCutting/
    │   ├── ControllerHandler/ ← Middleware global de exceções
    │   └── IoC/               ← Registro de dependências (DI.cs)
    └── 4.2-Data/
        ├── ProjectContext.cs  ← DbContext (Entity Framework Core)
        ├── Interfaces/        ← Contratos dos repositórios
        ├── Repositories/      ← Acesso ao banco de dados
        └── Mappers/           ← Configurações de tabela EF (IEntityTypeConfiguration)
```

### Fluxo de uma requisição

```
HTTP Request
    ↓
Controller          (01-Presentation)
    ↓
Service             (02-Application)  ← regra de negócio aqui
    ↓
Repository          (04-Infra/4.2-Data)
    ↓
ProjectContext      (Entity Framework Core + PostgreSQL)
    ↓
HTTP Response
```

### Tecnologias

| Tecnologia | Uso |
|---|---|
| .NET 9 | Framework principal |
| PostgreSQL 16 | Banco de dados |
| Entity Framework Core 9 | ORM + Migrations |
| AutoMapper | Mapeamento DTO ↔ Entidade |
| Swagger (Swashbuckle) | Documentação e teste da API |
| JWT Bearer | Autenticação via Keycloak |
| Docker | Banco de dados local |

---

## Configuração do Ambiente Local

### Pré-requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [EF Core CLI Tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)

Instalar o EF Core CLI (se ainda não tiver):
```bash
dotnet tool install --global dotnet-ef
```

Verificar versão instalada (deve ser >= 9.x):
```bash
dotnet ef --version
```

---

### Passo 1 — Instalar o Docker Desktop

1. Acesse [https://www.docker.com/products/docker-desktop](https://www.docker.com/products/docker-desktop)
2. Baixe para **Windows**
3. Instale e reinicie o computador se solicitado
4. Abra o Docker Desktop e aguarde o ícone na bandeja ficar **verde** (Engine running)

---

### Passo 2 — Subir o banco de dados com Docker

Abra um terminal **na pasta raiz do projeto** (onde está o `docker-compose.yml`) e execute:

```bash
docker compose up -d
```

O que esse comando faz:
- Baixa a imagem `postgres:16` do Docker Hub (só na primeira vez)
- Cria o container `apieventsr-db`
- Cria o banco `apieventsr_dev` automaticamente
- Expõe a porta `5432` no seu `localhost`
- O `-d` roda em segundo plano (detached)

**Banco criado com:**
| Configuração | Valor |
|---|---|
| Host | `localhost` |
| Porta | `5432` |
| Banco | `apieventsr_dev` |
| Usuário | `postgres` |
| Senha | `postgres` |

Verificar se o container está rodando:
```bash
docker ps
```

Você deve ver `apieventsr-db` com status `Up`.

Comandos úteis do dia a dia:
```bash
docker compose stop       # para o banco sem destruir os dados
docker compose start      # retoma o banco
docker compose down       # para e remove o container (dados do volume ficam)
docker compose down -v    # CUIDADO: apaga os dados também
```

---

### Passo 3 — Criar o appsettings.Development.json

O arquivo `appsettings.Development.json` **não vai para o Git** (está no `.gitignore`).  
Você precisa criá-lo manualmente a partir do template:

1. Copie o arquivo de exemplo:
```bash
cp src/01-Presentation/appsettings.Development.example.json src/01-Presentation/appsettings.Development.json
```

2. O arquivo já vem configurado para usar o Docker local — nenhuma alteração é necessária se você usou o `docker compose up`.

> Se você usa um PostgreSQL local com senha diferente, edite a `CONNECTION_STRING` no arquivo.

---

### Passo 4 — Rodar as migrations (criar as tabelas)

```bash
dotnet ef migrations add InitEventModule \
  --project src/04-Infra/4.2-Data \
  --startup-project src/01-Presentation
```

Aplicar ao banco:
```bash
dotnet ef database update \
  --project src/04-Infra/4.2-Data \
  --startup-project src/01-Presentation
```

---

### Passo 5 — Rodar a API

```bash
dotnet run --project src/01-Presentation
```

Acesse o Swagger em:
```
https://localhost:{porta}/swagger
```

> A porta é exibida no terminal quando a API sobe.

---

## Cards Atendidos

### ✅ Implementados (infraestrutura completa)

| Card | Descrição | O que foi feito |
|---|---|---|
| **85067** | Visualização da lista de eventos | `GET /api/v1/event/all` com status calculado e flag de inscrição |
| **85069** | Status de eventos | Motor de status (Em breve / Inscrição aberta / Em andamento / Encerrado) |
| **85071** | Informações gerais do evento | `GET /api/v1/event/{id}` com todos os campos |
| **85072** | Cronograma do evento | Cronograma derivado das datas, retornado no detalhe do evento |
| **85074** | Regulamento e mais informações | Entidade `EventDocument` + documentos no detalhe do evento |
| **85079** | Detalhes da premiação | Campo `AwardDetails` no detalhe do evento |

### ⏳ Pendentes

| Card | Descrição | Passo |
|---|---|---|
| **85075** | Acesso à inscrição (botão habilitado) | Passo 4 — flags `CanEnroll` já entregues no detalhe |
| **85077** | Formulário de inscrição | Passo 5 — `POST /event-enrollment` |
| **85078** | Submissão de arquivos | Passo 6 — `POST /file` + `POST /file/confirm` |
| **85081** | Finalizar inscrição | Passo 5/6 — parte do fluxo de criação |
| **85082** | Lista de projetos cadastrados | Passo 7 — `GET /event-enrollment/all` |
| **85293** | Permissões de edição e exclusão | Passo 7 — flags `IsEditable` e `IsDeletable` |
| **85083** | Editar projeto | Passo 8 — `PUT /event-enrollment/{id}` |
| **85084** | Excluir projeto | Passo 9 — `DELETE /event-enrollment/{id}` (lógico) |
| **85085** | Inscrever novo projeto | Passo 5 — reaproveitado do `POST /event-enrollment` |

---

## O que foi implementado

### Domínio — Entidades (`03-Domain/Entities/`)

| Arquivo | Responsabilidade |
|---|---|
| `BaseEntity.cs` | Base de todas as entidades: `Id`, `CreateDate`, `UpdateDate`, `DeleteDate` |
| `Event.cs` | Evento: título, descrição, banner, datas, detalhes de premiação |
| `EventDocument.cs` | Documento anexado ao evento (regulamento, edital) |
| `EventEnrollment.cs` | Inscrição de um projeto da escola no evento |
| `EnrollmentFile.cs` | Arquivo vinculado a uma inscrição |
| `Segment.cs` | Segmento disponível para inscrição |
| `Category.cs` | Categoria (sempre filha de um segmento) |
| `UserSegment.cs` | Vínculo entre usuário (JWT) e segmento (para Coordenador/Professor) |

### Domínio — Enums (`03-Domain/Enums/`)

| Arquivo | Valores |
|---|---|
| `EventStatus.cs` | `ComingSoon`, `EnrollmentOpen`, `InProgress`, `Closed` |
| `FileType.cs` | `Image`, `Pdf`, `Video` |
| `UserRole.cs` | `School`, `Coordinator`, `Professor` |

### Banco de dados — Tabelas criadas pelo EF

| Tabela | Entidade | Observação |
|---|---|---|
| `events` | `Event` | — |
| `event_documents` | `EventDocument` | — |
| `event_enrollments` | `EventEnrollment` | Índice único: `school_id + event_id + segment_id + category_id` (só ativos) |
| `enrollment_files` | `EnrollmentFile` | Índice único: `enrollment_id + original_name` (só ativos) |
| `segments` | `Segment` | — |
| `categories` | `Category` | FK para `segments` |
| `user_segments` | `UserSegment` | Índice único: `user_id + segment_id` |

### Endpoints implementados

#### `GET /api/v1/event/all`
Lista todos os eventos com status calculado.

**Resposta (200):**
```json
[
  {
    "id": "uuid",
    "title": "Nome do Evento",
    "bannerUrl": "https://...",
    "enrollmentStartDate": "2025-03-01T00:00:00Z",
    "enrollmentEndDate": "2025-03-31T00:00:00Z",
    "status": 1,
    "hasEnrollment": false,
    "isCurrent": true
  }
]
```

| Campo | Descrição |
|---|---|
| `status` | `0` = Em breve · `1` = Inscrição aberta · `2` = Em andamento · `3` = Encerrado |
| `hasEnrollment` | `true` se a escola do usuário logado já tem projeto inscrito |
| `isCurrent` | `true` para eventos não Encerrados — frontend usa para separar seções |

---

#### `GET /api/v1/event/{id}`
Detalhe completo de um evento.

**Resposta (200):**
```json
{
  "id": "uuid",
  "title": "Nome do Evento",
  "description": "Descrição completa...",
  "bannerUrl": "https://...",
  "status": 1,
  "hasEnrollment": false,
  "canEnroll": true,
  "canViewProjects": true,
  "awardDetails": "Descrição dos prêmios...",
  "documents": [
    { "id": "uuid", "name": "Regulamento.pdf", "url": "https://..." }
  ],
  "schedule": [
    { "label": "Início das inscrições", "date": "2025-03-01T00:00:00Z" },
    { "label": "Fim das inscrições",    "date": "2025-03-31T00:00:00Z" },
    { "label": "Divulgação",            "date": "2025-04-15T00:00:00Z" }
  ]
}
```

| Campo | Descrição |
|---|---|
| `canEnroll` | `true` somente quando `status == EnrollmentOpen` |
| `canViewProjects` | `true` quando status **não** é `ComingSoon` |
| `documents` | Ordenados alfabeticamente por nome |

---

### Infraestrutura adicionada

| Arquivo | O que faz |
|---|---|
| `ExceptionHandler.cs` | `KeyNotFoundException` → HTTP 404 automaticamente |
| `Startup.cs` | `UseAuthentication()` e `UseAuthorization()` adicionados ao pipeline |
| `DI.cs` | `IEventRepository` e `IEventService` registrados |
| `.gitignore` | Regras corrigidas para proteger `appsettings.Development.json` |

---

## Próximos Passos

### Plano de 20 dias

```
Semana 1 — Enrollment CRUD funcionando (meta do próximo 1:1)
  Dia 1: Banco local + migrations + Swagger rodando
  Dia 2: GET /event-enrollment/segment/{eventId}  (segmentos/categorias aptos)
  Dia 3: POST /event-enrollment  (criar inscrição com validações)
  Dia 4: GET /event-enrollment/all  (lista + flags IsEditable/IsDeletable)
  Dia 5: PUT /event-enrollment/{id}  (editar inscrição)

Semana 2 — Deleção + Upload
  Dia 6:  DELETE /event-enrollment/{id}  (exclusão lógica)
  Dia 7:  POST /file  (upload com validações de tipo e limite)
  Dia 8:  POST /file/confirm
  Dia 9:  DELETE /file/{fileId}
  Dia 10: Teste ponta-a-ponta do fluxo completo

Semana 3 — Regras de negócio + Permissões
  Dia 11: Regras de limite de arquivo (6 total, 4 imgs, 1 PDF, 1 vídeo)
  Dia 12: Regras de permissão por perfil
  Dia 13: Validação de duplicidade escola+evento+segmento+categoria
  Dia 14: Refinamento de mensagens de erro
  Dia 15: Alinhamento com QA/PO

Semana 4 — Testes + Entrega
  Dia 16-17: Testes dos cenários críticos
  Dia 18:    Ajustes pós-revisão
  Dia 19:    Code review + Swagger completo
  Dia 20:    Buffer / deploy / apresentação
```

### Próximo endpoint a implementar
**`GET /api/v1/event-enrollment/segment/{eventId}`** — retorna segmentos e categorias disponíveis para inscrição, considerando o perfil do usuário logado.

---

## Como Testar

### Via Swagger (recomendado para desenvolvimento)

1. Suba a API: `dotnet run --project src/01-Presentation`
2. Acesse: `https://localhost:{porta}/swagger`
3. Clique em **Authorize** e informe o Bearer token do Keycloak
4. Expanda o endpoint desejado → **Try it out** → **Execute**

### Cenários de teste — `GET /event/all`

| Cenário | Comportamento esperado |
|---|---|
| Evento com `EnrollmentStartDate` no futuro | `status: 0` (Em breve) |
| Evento com datas de inscrição abertas agora | `status: 1` (Inscrição aberta) |
| Inscrição encerrada, resultado não divulgado | `status: 2` (Em andamento) |
| Data de divulgação já passou | `status: 3` (Encerrado) |
| Escola tem projeto inscrito no evento | `hasEnrollment: true` |
| Evento encerrado | `isCurrent: false` |

### Cenários de teste — `GET /event/{id}`

| Cenário | Comportamento esperado |
|---|---|
| ID inexistente | HTTP `404` com `{ "error": "Evento {id} não encontrado." }` |
| Evento sem documentos | `documents: []` |
| Status `EnrollmentOpen` | `canEnroll: true` |
| Qualquer outro status | `canEnroll: false` |
| Status `ComingSoon` | `canViewProjects: false` |

---

## Decisões Técnicas

| Decisão | Justificativa |
|---|---|
| **Status calculado, não persistido** | Status muda com o tempo — persistir criaria inconsistência. Calculado no service a cada requisição. |
| **`<=` e `>=` na virada de status** | Convencional: quando a data chega, o estado já virou. Acordado para simplificar sem input do PO. |
| **Deleção lógica via `DeleteDate`** | A `BaseEntity` já tem o campo. Ao fazer `Delete()`, o registro fica no banco mas filtrado em todas as queries. |
| **`schoolId` e `userId` vindos do JWT** | Não existe entidade `User` local. Identidade vem dos claims do token Keycloak. |
| **Segmento/Categoria em endpoints separados** | Padrão mais comum em formulários com seleção encadeada (UX de step). |
| **Upload em duas etapas (POST + confirm)** | Evidência no board: `POST /file` sobe para storage, `POST /file/confirm` registra no banco. |
| **Constraint única com filtro `IS NULL`** | A regra "escola não pode ter duplicata ativa" ignora registros excluídos logicamente. |
| **`KeyNotFoundException` → HTTP 404** | Mapeado no `ExceptionHandler` global — o controller não precisa tratar manualmente. |
