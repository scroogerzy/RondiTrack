import { getSession } from '@/lib/session';
import { apiUrl } from '@/lib/api';
import { randomUUID } from 'node:crypto';
export async function POST(request: Request, { params }: RouteContext<'/bff/stokvels/[id]/cycles/[cycleId]/close'>) {
  const session = await getSession(); if (!session) return Response.json({ title: 'Your session has ended.' }, { status: 401 });
  const { id, cycleId } = await params;
  const correlationId = request.headers.get('X-Correlation-ID') ?? randomUUID();
  let upstream: Response;
  try { upstream = await fetch(apiUrl(`/api/v1/stokvels/${id}/cycles/${cycleId}/close`), { method: 'POST', headers: { Authorization: `Bearer ${session.accessToken}`, 'Content-Type': 'application/json', 'Idempotency-Key': request.headers.get('Idempotency-Key') ?? '', 'X-Correlation-ID': correlationId }, body: await request.text(), cache: 'no-store' }); }
  catch { return Response.json({ detail: 'The close service could not be reached.', correlationId }, { status: 502, headers: { 'X-Correlation-ID': correlationId, 'Cache-Control': 'no-store' } }); }
  return new Response(await upstream.arrayBuffer(), { status: upstream.status, headers: { 'Content-Type': upstream.headers.get('Content-Type') ?? 'application/json', 'Cache-Control': 'no-store', 'X-Correlation-ID': upstream.headers.get('X-Correlation-ID') ?? correlationId } });
}
