-- Seed mínimo para destravar inscrição e upload em ambiente local.
-- Execute após migrations: psql -h localhost -U postgres -d apieventsr_dev -f Tarefas/seed_dados_minimos_evento.sql

BEGIN;

-- IDs fixos para facilitar token e chamadas de API
-- school_id: 11111111-1111-1111-1111-111111111111
-- user_id  : 22222222-2222-2222-2222-222222222222

INSERT INTO events (
    "Id", "Title", "Description", "BannerUrl",
    "EnrollmentStartDate", "EnrollmentEndDate", "ResultDate", "AwardDetails",
    "CreateDate", "UpdateDate", "DeleteDate"
)
VALUES (
    'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
    'Evento Demo API',
    'Evento de demonstração para validar inscrição e upload.',
    NULL,
    NOW() - INTERVAL '1 day',
    NOW() + INTERVAL '7 day',
    NOW() + INTERVAL '30 day',
    'Premiação demo',
    NOW(),
    NULL,
    NULL
)
ON CONFLICT ("Id") DO UPDATE SET
    "Title" = EXCLUDED."Title",
    "Description" = EXCLUDED."Description",
    "EnrollmentStartDate" = EXCLUDED."EnrollmentStartDate",
    "EnrollmentEndDate" = EXCLUDED."EnrollmentEndDate",
    "ResultDate" = EXCLUDED."ResultDate",
    "DeleteDate" = NULL;

INSERT INTO segments ("Id", "Name", "IsActive", "CreateDate", "UpdateDate", "DeleteDate")
VALUES
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1', 'Fundamental II', TRUE, NOW(), NULL, NULL),
    ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2', 'Ensino Médio', TRUE, NOW(), NULL, NULL)
ON CONFLICT ("Id") DO UPDATE SET
    "Name" = EXCLUDED."Name",
    "IsActive" = TRUE,
    "DeleteDate" = NULL;

INSERT INTO categories ("Id", "Name", "IsActive", "SegmentId", "CreateDate", "UpdateDate", "DeleteDate")
VALUES
    ('cccccccc-cccc-cccc-cccc-ccccccccccc1', 'Matemática', TRUE, 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1', NOW(), NULL, NULL),
    ('cccccccc-cccc-cccc-cccc-ccccccccccc2', 'Ciências', TRUE, 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2', NOW(), NULL, NULL)
ON CONFLICT ("Id") DO UPDATE SET
    "Name" = EXCLUDED."Name",
    "IsActive" = TRUE,
    "SegmentId" = EXCLUDED."SegmentId",
    "DeleteDate" = NULL;

INSERT INTO user_segments ("Id", "UserId", "SegmentId", "CreateDate", "UpdateDate", "DeleteDate")
VALUES
    ('dddddddd-dddd-dddd-dddd-ddddddddddd1', '22222222-2222-2222-2222-222222222222', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb1', NOW(), NULL, NULL),
    ('dddddddd-dddd-dddd-dddd-ddddddddddd2', '22222222-2222-2222-2222-222222222222', 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbb2', NOW(), NULL, NULL)
ON CONFLICT ("UserId", "SegmentId") DO UPDATE SET
    "DeleteDate" = NULL;

COMMIT;
