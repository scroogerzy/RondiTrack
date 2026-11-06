import { Suspense } from 'react';
import { signedInApi, readOrFail } from '@/lib/api';
import ActivityFeedClient from './ActivityFeedClient';

type ActivityItem = {
  id: string; stokvelId: string; stokvelName: string; type: string;
  occurredAtUtc: string; summary: string; relatedUserId: string | null;
  relatedUserName: string | null; amountInCents: number | null; cycleNumber: number | null;
};
type Page = { items: ActivityItem[]; nextPageToken: string; pageSize: number };

async function ActivityContent() {
  const response = await signedInApi('/api/v1/activity?pageSize=20');
  const initialPage = await readOrFail<Page>(response);
  return <ActivityFeedClient initialPage={initialPage} />;
}

export default function ActivityPage() {
  return (
    <Suspense fallback={<div className="animate-pulse space-y-4"><div className="h-8 w-1/3 rounded bg-slate-200" /><div className="h-64 rounded-xl bg-slate-100" /></div>}>
      <ActivityContent />
    </Suspense>
  );
}
