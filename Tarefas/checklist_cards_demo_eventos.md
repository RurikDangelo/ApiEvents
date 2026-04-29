# Checklist de apresentação — Eventos (cards críticos)

> Versão revisada 29/04/2026. Cruzamento dos cards com o código real.  
> Documentação técnica completa em `Tarefas/REVISAO_PROJETO.md`.  
> Roteiro de execução em `Tarefas/GUIA_EXECUCAO.md`.

---

## Status geral

| # | Card | Status backend | Observação |
|---|---|---|---|
| 1 | 85067 — Lista de eventos | Atendido | `GET /event/all` |
| 2 | 85069 — Status de eventos | Atendido | `EventService.CalculateStatus` |
| 3 | 85071 — Informações gerais | Atendido | `GET /event/{id}` |
| 4 | 85072 — Cronograma | **Parcial** | só 3 etapas das 5 pedidas |
| 5 | 85074 — Regulamento e docs | Atendido | docs ordenados |
| 6 | 85075 — Acesso à inscrição | Atendido (backend) | flag `canEnroll` |
| 7 | 85077 — Formulário de inscrição | Atendido (backend) | `POST /event-enrollment` |
| 8 | 85078 — Submissão de arquivos | **Parcial** | 3 gaps (ver abaixo) |
| 9 | 85079 — Detalhes da premiação | **Parcial** | mesmo gap de cronograma |
| 10 | 85081 — Finalizar inscrição | Atendido (backend) | criação atômica |
| 11 | 85082 — Lista de projetos | Atendido (backend) | `GET /event-enrollment/all` |
| 12 | 85083 — Editar projeto | Atendido (backend) | `PUT /event-enrollment/{id}` |
| 13 | 85084 — Excluir projeto | Atendido | soft delete |
| 14 | 85085 — Inscrever novo projeto | Atendido | reaproveita `POST` |
| 15 | 85293 — Permissões | Atendido | filtros por role |

---

## Detalhes dos cards parciais

### Card 85072 — Cronograma do evento (Parcial)
**Implementado:** `EventService.BuildSchedule` retorna 3 etapas: "Início das inscrições", "Fim das inscrições", "Divulgação".

**Faltando:**
- Etapa "Avaliação dos trabalhos" (CA_03)
- Etapa "Cerimônia de premiação" (CA_05)
- Etapa "Divulgação dos premiados no P+" (CA_06)

**Ação:** adicionar campos em `Event` (`EvaluationStartDate`, `EvaluationEndDate`, `AwardCeremonyInfo`, `PMaisDisclosureDate`), nova migration e atualizar `BuildSchedule`.

---

### Card 85078 — Submissão de arquivos (Parcial)
**Implementado:**
- Limites: 6 total, 4 imagens, 1 PDF, 1 vídeo (`EnrollmentFileService`)
- Tipos MIME validados (JPEG/PNG/GIF/WEBP/PDF/MP4/WEBM/MOV)
- Bloqueio de nome duplicado (service + índice único no banco)
- Soft delete + remoção física

**Faltando:**
1. **Duração de vídeo ≤1min** (CA_07) — não há validação.
2. **Nome no formato `idEscola_idProjeto_nomeArquivo`** (CA_13) — código usa `{timestamp}_{nome}`.
3. **Ordenação alfabética** (CA_09) — `EnrollmentFileRepository` ordena por `CreateDate`.
4. **`GET /event-enrollment/{enrollmentId}/files`** — endpoint de listagem ainda não existe.

---

### Card 85079 — Detalhes da premiação (Parcial)
- `awardDetails` é exposto pelo `GET /event/{id}`.
- Cronograma compartilha o gap do card 85072.

---

## Cards atendidos com evidência

| Card | Endpoint principal | Validações chave |
|---|---|---|
| 85067 | `GET /event/all` | retorna banner, título, datas, status, `hasEnrollment`, `isCurrent` |
| 85069 | `EventService.CalculateStatus` | 4 status (`<` / `<=`) |
| 85071 | `GET /event/{id}` | banner, título, descrição, docs, cronograma, `canEnroll`, `canViewProjects` |
| 85074 | `GET /event/{id}` | `Documents.OrderBy(Name)` |
| 85075 | `GET /event/{id}` | `canEnroll = status == EnrollmentOpen` |
| 85077 | `POST /event-enrollment` | obrigatórios + duplicidade + segmento/categoria ativos |
| 85081 | `POST /event-enrollment` | criação atômica |
| 85082 | `GET /event-enrollment/all` | flags `CanEdit`/`CanDelete`/`CanUpload` por evento e perfil |
| 85083 | `PUT /event-enrollment/{id}` | bloqueia alterar evento/segmento/categoria |
| 85084 | `DELETE /event-enrollment/{id}` | soft delete (`DeleteDate`) |
| 85085 | `POST /event-enrollment` | reaproveitado |
| 85293 | `EventEnrollmentService` | regras Escola/Coordenador/Professor + bloqueio em evento encerrado |

---

## Evidência de execução real (29/04/2026)

Fluxo automatizado executado com sucesso via `Tarefas/executar_fluxo_demo_eventos.ps1` após ajustes de compatibilidade local (HTTPS/JWT/PowerShell 5):

- `EnrollmentID`: `b2d25942-455e-42ec-b270-2e0e4eb55164`
- `FileID`: `728db93f-6e3e-462b-8752-5792b3c265c7`
- `EventId`: `aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa`
- `SegmentId`: `bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2`
- `CategoryId`: `cccccccc-cccc-cccc-cccc-ccccccccccc2`
- Evidência em disco: `C:\uploads\apieventsr\b2d25942-455e-42ec-b270-2e0e4eb55164\1777479387_demo-upload-20260429131627.png`

Validação adicional feita por chamada direta no endpoint de upload:

- `EnrollmentID`: `9ede049a-5cce-4a59-9c9f-90f46d220894`
- `FileID`: `c525aef6-733d-475f-abcb-b50281fa7ea2`

---

## Inconsistências resolvidas neste documento

- **IDs do `apieventsr/GUIA-DE-TESTES.md` divergiam do seed** (`cccccccc-…000002` vs `aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa`). O `GUIA_EXECUCAO.md` agora usa os IDs reais do `seed_dados_minimos_evento.sql`.
- **`appsettings.Development.json`** está no gitignore. O `GUIA_EXECUCAO.md` instrui a copiar o `.example.json`.
- **Tabelas não criadas no banco do time** se resolve aplicando a migration uma vez (`dotnet ef database update`).

---

## Plano de execução para hoje

1. `docker compose up -d`
2. `dotnet ef database update`
3. Carregar seed via `psql` no container
4. Validar tabelas no DBeaver
5. `dotnet run --project src/01-Presentation`
6. Gerar JWT no jwt.io
7. Criar inscrição via Swagger (capturar `EnrollmentID`)
8. Upload de imagem
9. Conferir arquivo no disco e no banco
