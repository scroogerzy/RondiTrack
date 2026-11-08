import { getSession } from '@/lib/session';
import { apiUrl } from '@/lib/api';
export async function GET(_request: Request, { params }: RouteContext<'/bff/stokvels/[id]/cycles/[cycleId]/close-preview'>) {
  const session = await getSession(); if (!session) return Response.json({ title: 'Your session has ended.' }, { status: 401 });
  const { id, cycleId } = await params;
  const upstream = await fetch(apiUrl(`/api/v1/stokvels/${id}/cycles/${cycleId}/close-preview`), { headers: { Authorization: `Bearer ${session.accessToken}` }, cache: 'no-store' });
  return new Response(await upstream.arrayBuffer(), { status: upstream.status, headers: { 'Content-Type': upstream.headers.get('Content-Type') ?? 'application/json', 'Cache-Control': 'no-store' } });
}
