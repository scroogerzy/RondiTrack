import { context, trace } from '@opentelemetry/api';

type MutableEvent = Record<string, unknown>;

function asRecord(value: unknown): MutableEvent | undefined {
  return value && typeof value === 'object' ? value as MutableEvent : undefined;
}

/** Keep only correlation-safe trace context; remove all user/request content. */
export function scrubSentryEvent<T extends object>(event: T): T {
  const target = event as MutableEvent;
  const contexts = asRecord(target.contexts);
  const rawTraceContext = contexts?.trace;

  target.user = undefined;
  target.request = undefined;
  target.breadcrumbs = [];
  target.extra = {};
  target.message = 'Application request failed.';
  target.transaction = 'ronditrack.web';

  const tags = asRecord(target.tags);
  const traceContext = asRecord(rawTraceContext);
  const activeTraceId = trace.getSpan(context.active())?.spanContext().traceId;
  const traceId = activeTraceId ?? traceContext?.trace_id ?? tags?.trace_id;
  target.tags = traceId ? { trace_id: traceId } : {};
  target.contexts = traceId ? { trace: { trace_id: traceId } } : {};

  const exception = asRecord(target.exception);
  if (exception && Array.isArray(exception.values)) {
    exception.values = exception.values.map((item) => {
      const value = asRecord(item);
      return value ? { ...value, value: 'Application request failed.' } : item;
    });
    target.exception = exception;
  }

  return event;
}
