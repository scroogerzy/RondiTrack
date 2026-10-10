-- Run the same plan shape against a non-volume contribution already in the
-- development database. \\gexec executes the generated EXPLAIN statement.
-- This file is run once before the 10k seed, and once after seed cleanup with
-- the new index installed. It never fabricates row counts or execution times.
SELECT format(
$plan$
EXPLAIN (ANALYZE, BUFFERS, VERBOSE)
SELECT c."Id", c."Amount", c."Cycle", c."RecordedAt", c."StokvelId", c."UserId"
FROM public."Contributions" AS c
WHERE c."StokvelId" = %L::uuid
  AND c."Cycle" = %s
ORDER BY c."RecordedAt" DESC, c."Id" DESC
LIMIT 21;
$plan$,
    sample."StokvelId",
    sample."Cycle")
FROM (
    SELECT "StokvelId", "Cycle"
    FROM public."Contributions"
    ORDER BY "RecordedAt" DESC
    LIMIT 1
) AS sample
\gexec
