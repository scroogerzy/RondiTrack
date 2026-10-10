-- Bitcube Assignment 5.3 volume seeder.
-- Explicitly run this file when measuring query plans; Program.cs must NOT call it.
-- Creates 100 stokvels x 20 members x 5 cycles = exactly 10,000 contributions.
-- All generated identifiers are stable, so rerunning the script is safe.
BEGIN;

INSERT INTO public."Stokvels" ("Id", "Name", "MonthlyContribution")
SELECT md5('RondiTrack-Assignment-5.3-Stokvel-' || s)::uuid,
       'Assignment 5.3 Volume Stokvel ' || s,
       500.00
FROM generate_series(1, 100) AS s
ON CONFLICT DO NOTHING;

INSERT INTO public."Users" ("Id", "FullName", "Email")
SELECT md5('RondiTrack-Assignment-5.3-User-' || u)::uuid,
       'Assignment 5.3 Volume User ' || u,
       'assignment53.user.' || u || '@example.test'
FROM generate_series(1, 2000) AS u
ON CONFLICT DO NOTHING;

INSERT INTO public."StokvelMembers" ("UserId", "StokvelId", "JoinedAtUtc", "Role")
SELECT md5('RondiTrack-Assignment-5.3-User-' || ((s - 1) * 20 + member_no))::uuid,
       md5('RondiTrack-Assignment-5.3-Stokvel-' || s)::uuid,
       TIMESTAMPTZ '2026-01-01 00:00:00+00' + ((s + member_no) * INTERVAL '1 minute'),
       'Member'
FROM generate_series(1, 100) AS s
CROSS JOIN generate_series(1, 20) AS member_no
ON CONFLICT DO NOTHING;

INSERT INTO public."ContributionCycles"
    ("Id", "EndDate", "PeriodNumber", "StartDate", "StokvelId", "TargetAmount")
SELECT md5('RondiTrack-Assignment-5.3-Cycle-' || s || '-' || period_no)::uuid,
       (DATE '2026-01-01' + ((period_no - 1) * INTERVAL '1 month') + INTERVAL '1 month')::timestamp,
       period_no,
       (DATE '2026-01-01' + ((period_no - 1) * INTERVAL '1 month'))::timestamp,
       md5('RondiTrack-Assignment-5.3-Stokvel-' || s)::uuid,
       10000.00
FROM generate_series(1, 100) AS s
CROSS JOIN generate_series(1, 5) AS period_no
ON CONFLICT DO NOTHING;

INSERT INTO public."Contributions"
    ("Id", "Amount", "Cycle", "RecordedAt", "StokvelId", "UserId")
SELECT md5('RondiTrack-Assignment-5.3-Contribution-' || s || '-' || period_no || '-' || member_no)::uuid,
       500.00,
       period_no,
       TIMESTAMPTZ '2026-01-01 00:00:00+00'
           + (((period_no - 1) * 30 + member_no) * INTERVAL '1 day'),
       md5('RondiTrack-Assignment-5.3-Stokvel-' || s)::uuid,
       md5('RondiTrack-Assignment-5.3-User-' || ((s - 1) * 20 + member_no))::uuid
FROM generate_series(1, 100) AS s
CROSS JOIN generate_series(1, 5) AS period_no
CROSS JOIN generate_series(1, 20) AS member_no
ON CONFLICT DO NOTHING;

COMMIT;

SELECT COUNT(*) AS seeded_contributions
FROM public."Contributions"
WHERE "Id" IN (
    SELECT md5('RondiTrack-Assignment-5.3-Contribution-' || s || '-' || period_no || '-' || member_no)::uuid
    FROM generate_series(1, 100) AS s
    CROSS JOIN generate_series(1, 5) AS period_no
    CROSS JOIN generate_series(1, 20) AS member_no
);
