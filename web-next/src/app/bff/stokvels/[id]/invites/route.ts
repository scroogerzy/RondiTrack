import { getSession } from '@/lib/session';
import { apiUrl } from '@/lib/api';
export async function POST(_request: Request, { params }: RouteContext<'/bff/stokvels/[id]/invites'>) {
  const session = await getSession(); if (!session) return Response.json({ title: 'Your session has ended.' }, { status: 401 });
  const { id } = await params;
  const upstream = await fetch(apiUrl(`/api/v1/stokvels/${id}/invites`), { method: 'POST', headers: { Authorization: `Bearer ${session.accessToken}` }, cache: 'no-store' });
  return new Response(await upstream.arrayBuffer(), { status: upstream.status, headers: { 'Content-Type': upstream.headers.get('Content-Type') ?? 'application/json', 'Cache-Control': 'no-store' } });
}
