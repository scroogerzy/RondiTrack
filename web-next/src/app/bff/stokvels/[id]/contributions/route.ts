import { NextRequest, NextResponse } from 'next/server';
import { getSession } from '@/lib/session';
import { apiUrl } from '@/lib/api';
import { loadContributionHistoryFilters } from '@/lib/contributionHistoryFilters';
import { southAfricanDaysToUtcRange } from '@/lib/southAfricanDateRange';

export async function GET(
  request: NextRequest,
  context: { params: Promise<{ id: string }> }
) {
  const session = await getSession();
  if (!session) {
    return NextResponse.json({ title: 'Your session has ended.' }, { status: 401 });
  }

  const { id } = await context.params;
  if (!/^[0-9a-f]{8}-[0-9a-f]{4}-[1-8][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(id)) {
    return NextResponse.json({ title: 'Stokvel was not found.' }, { status: 404 });
  }

  const url = new URL(request.url);
  const allowed = new Set(['member', 'from', 'to', 'direction', 'pageSize', 'pageToken']);
  const unknown = [...url.searchParams.keys()].find(key => !allowed.has(key));
  if (unknown) {
    return NextResponse.json(
      { title: `Unsupported query parameter '${unknown}'.` },
      { status: 400 }
    );
  }

  // The same allow-list parser is used by the page, controls and this Route Handler.
  const filters = await loadContributionHistoryFilters({
    member: url.searchParams.get('member') ?? undefined,
    from: url.searchParams.get('from') ?? undefined,
    to: url.searchParams.get('to') ?? undefined,
    direction: url.searchParams.get('direction') ?? undefined
  });

  let range: ReturnType<typeof southAfricanDaysToUtcRange>;
  try {
    range = southAfricanDaysToUtcRange(filters.from, filters.to);
  } catch {
    return NextResponse.json(
      { title: 'The end date must be on or after the start date.' },
      { status: 400 }
    );
  }

  const pageSizeValue = Number(url.searchParams.get('pageSize') ?? '20');
  if (!Number.isInteger(pageSizeValue) || pageSizeValue < 1 || pageSizeValue > 100) {
    return NextResponse.json({ title: 'pageSize must be between 1 and 100.' }, { status: 400 });
  }

  const target = new URL(apiUrl(`/api/v1/stokvels/${id}/contributions`));
  target.searchParams.set('pageSize', String(pageSizeValue));
  target.searchParams.set('sortDirection', filters.direction);
  if (filters.member) target.searchParams.set('userId', filters.member);
  if (range.fromUtc) target.searchParams.set('from', range.fromUtc);
  if (range.toUtcExclusive) target.searchParams.set('to', range.toUtcExclusive);
  const pageToken = url.searchParams.get('pageToken');
  if (pageToken) target.searchParams.set('pageToken', pageToken);

  const response = await fetch(target, {
    method: 'GET',
    cache: 'no-store',
    headers: { Authorization: `Bearer ${session.accessToken}` }
  });

  return new Response(await response.text(), {
    status: response.status,
    headers: {
      'Content-Type': response.headers.get('content-type') ?? 'application/json',
      'Cache-Control': 'no-store'
    }
  });
}
