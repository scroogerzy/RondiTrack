import { getSession } from '@/lib/session';
import { apiUrl } from '@/lib/api';
export async function PUT(request: Request, { params }: RouteContext<'/bff/stokvels/[id]/rules'>) {
  const session = await getSession(); if (!session) return Response.json({ title: 'Your session has ended.' }, { status: 401 });
  const { id } = await params;
  const upstream = await fetch(apiUrl(`/api/v1/stokvels/${id}/rules`), { method: 'PUT', headers: { Authorization: `Bearer ${session.accessToken}`, 'Content-Type': 'application/json' }, body: await request.text(), cache: 'no-store' });
  return new Response(null, { status: upstream.status, headers: { 'Cache-Control': 'no-store' } });
}
