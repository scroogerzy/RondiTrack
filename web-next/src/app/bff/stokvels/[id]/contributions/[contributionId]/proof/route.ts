import { getSession } from '@/lib/session';
import { apiUrl } from '@/lib/api';
import { randomUUID } from 'node:crypto';

export async function POST(request: Request, { params }: RouteContext<'/bff/stokvels/[id]/contributions/[contributionId]/proof'>) {
  const session = await getSession();
  if (!session) return Response.json({ title: 'Your session has ended.' }, { status: 401 });
  const { id, contributionId } = await params;
  const incoming = await request.formData();
  const file = incoming.get('file');
  if (!(file instanceof File) || file.size <= 0 || file.size > 5 * 1024 * 1024) return Response.json({ field: 'file', detail: 'Choose a file up to 5 MB.' }, { status: 400 });
  const outgoing = new FormData(); outgoing.set('file', file, file.name);
  const correlationId = randomUUID();
  let upstream: Response;
  try { upstream = await fetch(apiUrl(`/api/v1/stokvels/${id}/contributions/${contributionId}/proof`), { method: 'POST', headers: { Authorization: `Bearer ${session.accessToken}`, 'X-Correlation-ID': correlationId }, body: outgoing, cache: 'no-store' }); }
  catch { return Response.json({ detail: 'The upload service could not be reached.', correlationId }, { status: 502, headers: { 'X-Correlation-ID': correlationId, 'Cache-Control': 'no-store' } }); }
  const body = await upstream.arrayBuffer();
  return new Response(body, { status: upstream.status, headers: { 'Content-Type': upstream.headers.get('Content-Type') ?? 'application/json', 'Cache-Control': 'no-store', 'X-Correlation-ID': upstream.headers.get('X-Correlation-ID') ?? correlationId } });
}

export async function GET(_request: Request, { params }: RouteContext<'/bff/stokvels/[id]/contributions/[contributionId]/proof'>) {
  const session = await getSession();
  if (!session) return new Response('Unauthorised', { status: 401 });
  const { id, contributionId } = await params;
  const upstream = await fetch(apiUrl(`/api/v1/stokvels/${id}/contributions/${contributionId}/proof`), { headers: { Authorization: `Bearer ${session.accessToken}` }, cache: 'no-store' });
  if (!upstream.ok || !upstream.body) return new Response('Not found.', { status: upstream.status === 404 ? 404 : upstream.status });
  return new Response(upstream.body, { headers: { 'Content-Type': upstream.headers.get('Content-Type') ?? 'application/octet-stream', 'Content-Disposition': upstream.headers.get('Content-Disposition') ?? 'attachment', 'Cache-Control': 'private, no-store', 'X-Content-Type-Options': 'nosniff' } });
}

export async function DELETE(_request: Request, { params }: RouteContext<'/bff/stokvels/[id]/contributions/[contributionId]/proof'>) {
  const session = await getSession();
  if (!session) return Response.json({ title: 'Your session has ended.' }, { status: 401 });
  const { id, contributionId } = await params;
  const upstream = await fetch(apiUrl(`/api/v1/stokvels/${id}/contributions/${contributionId}/proof`), { method: 'DELETE', headers: { Authorization: `Bearer ${session.accessToken}` }, cache: 'no-store' });
  return new Response(null, { status: upstream.status, headers: { 'Cache-Control': 'no-store' } });
}
