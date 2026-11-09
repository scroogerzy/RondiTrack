import { NextRequest, NextResponse } from 'next/server';
import { getSession } from '@/lib/session';
import { apiUrl } from '@/lib/api';

const privateHeaders = {
  'Cache-Control': 'no-store',
  'X-Robots-Tag': 'noindex, nofollow, noarchive'
};

export async function GET(request: NextRequest) {
  const session = await getSession();
  if (!session) {
    return NextResponse.json(
      { title: 'Your session has ended.' },
      { status: 401, headers: privateHeaders }
    );
  }

  const target = new URL(apiUrl('/api/v1/activity'));
  new URL(request.url).searchParams.forEach((value, key) => target.searchParams.set(key, value));
  const response = await fetch(target, {
    method: 'GET',
    cache: 'no-store',
    headers: { Authorization: `Bearer ${session.accessToken}` }
  });

  return new Response(await response.text(), {
    status: response.status,
    headers: {
      ...privateHeaders,
      'Content-Type': response.headers.get('content-type') ?? 'application/json'
    }
  });
}
