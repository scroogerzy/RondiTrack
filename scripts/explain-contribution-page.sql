-- Same projection, equality filters, deterministic ordering and pageSize + 1 limit
-- emitted by EfContributionRepository.GetPageByCycleAsync for the first page.
-- The md5 expression resolves to the stable seeded stokvel's UUID.
EXPLAIN (ANALYZE, BUFFERS, VERBOSE)
SELECT c."Id", c."Amount", c."Cycle", c."RecordedAt", c."StokvelId", c."UserId"
FROM public."Contributions" AS c
WHERE c."StokvelId" = md5('RondiTrack-Assignment-5.3-Stokvel-1')::uuid
  AND c."Cycle" = 1
ORDER BY c."RecordedAt" DESC, c."Id" DESC
LIMIT 21;
