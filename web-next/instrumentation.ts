import { registerOTel } from '@vercel/otel';
import * as Sentry from '@sentry/nextjs';

export async function register() {
  if (process.env.NEXT_RUNTIME === 'nodejs') {
    await import('./sentry.server.config');
  }
  if (process.env.NEXT_RUNTIME === 'edge') {
    await import('./sentry.edge.config');
  }

  const apiBaseUrl = process.env.API_BASE_URL;
  registerOTel({
    serviceName: 'ronditrack-web',
    instrumentationConfig: {
      fetch: {
        // Only propagate W3C trace context to the private API origin.
        propagateContextUrls: apiBaseUrl ? [apiBaseUrl] : [],
      },
    },
  });
}

export const onRequestError = Sentry.captureRequestError;
