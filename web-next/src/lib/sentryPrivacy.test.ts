import { describe, expect, it } from 'vitest';
import { scrubSentryEvent } from './sentryPrivacy';

describe('scrubSentryEvent', () => {
  it('removes personal and request data while retaining the trace reference', () => {
    const traceId = '0123456789abcdef0123456789abcdef';
    const event = {
      user: { email: 'member@example.test', username: 'Member Name' },
      request: { url: '/bff/contributions?amount=500', headers: { cookie: 'secret' }, data: { amount: 500 } },
      breadcrumbs: [{ message: 'Recorded R500 for Member Name' }],
      extra: { accountNumber: '123456789', amount: 500 },
      message: 'Member Name contribution R500 failed',
      transaction: '/stokvels/private-id',
      tags: { trace_id: traceId, email: 'member@example.test' },
      contexts: { trace: { trace_id: traceId }, os: { name: 'Linux' } },
      exception: { values: [{ type: 'Error', value: 'Member Name paid R500' }] },
    };

    const safe = scrubSentryEvent(event);
    expect(safe.user).toBeUndefined();
    expect(safe.request).toBeUndefined();
    expect(safe.breadcrumbs).toEqual([]);
    expect(safe.extra).toEqual({});
    expect(safe.message).toBe('Application request failed.');
    expect(safe.tags).toEqual({ trace_id: traceId });
    expect(safe.contexts).toEqual({ trace: { trace_id: traceId } });
    expect(safe.exception.values[0].value).toBe('Application request failed.');
  });
});
