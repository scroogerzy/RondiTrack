-- Remove ONLY the deterministic rows created by the Assignment 5.3 seeder.
-- It is used temporarily to measure the existing small development dataset.
-- All non-seeder users, stokvels, contributions and cycles are retained.
BEGIN;

DELETE FROM public."Contributions"
WHERE "Id" IN (
    SELECT md5('RondiTrack-Assignment-5.3-Contribution-' || s || '-' || period_no || '-' || member_no)::uuid
    FROM generate_series(1, 100) AS s
    CROSS JOIN generate_series(1, 5) AS period_no
    CROSS JOIN generate_series(1, 20) AS member_no
);

DELETE FROM public."StokvelMembers"
WHERE "StokvelId" IN (
    SELECT md5('RondiTrack-Assignment-5.3-Stokvel-' || s)::uuid
    FROM generate_series(1, 100) AS s
);

DELETE FROM public."ContributionCycles"
WHERE "Id" IN (
    SELECT md5('RondiTrack-Assignment-5.3-Cycle-' || s || '-' || period_no)::uuid
    FROM generate_series(1, 100) AS s
    CROSS JOIN generate_series(1, 5) AS period_no
);

DELETE FROM public."Users"
WHERE "Id" IN (
    SELECT md5('RondiTrack-Assignment-5.3-User-' || u)::uuid
    FROM generate_series(1, 2000) AS u
);

DELETE FROM public."Stokvels"
WHERE "Id" IN (
    SELECT md5('RondiTrack-Assignment-5.3-Stokvel-' || s)::uuid
    FROM generate_series(1, 100) AS s
);

COMMIT;

ANALYZE public."Contributions";
