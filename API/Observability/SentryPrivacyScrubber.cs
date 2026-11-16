using Sentry;
using Sentry.Protocol;

namespace RondiTrack.API.Observability;

public static class SentryPrivacyScrubber
{
    public static SentryEvent? Scrub(SentryEvent sentryEvent)
    {
        sentryEvent.User = null!;
        sentryEvent.Request = null!;
        sentryEvent.Message = null;
        sentryEvent.TransactionName = "ronditrack.api";

        foreach (var key in sentryEvent.Tags.Keys.Where(key => key != "trace_id").ToArray())
            sentryEvent.UnsetTag(key);

        sentryEvent.Contexts.Clear();

        if (sentryEvent.Extra is IDictionary<string, object?> extra)
            extra.Clear();

        if (sentryEvent.Breadcrumbs is ICollection<Breadcrumb> breadcrumbs)
            breadcrumbs.Clear();

        if (sentryEvent.SentryExceptions is not null)
        {
            var safeExceptions = sentryEvent.SentryExceptions.Select(exception =>
            {
                exception.Value = "Application request failed.";
                return exception;
            }).ToArray();
            sentryEvent.SentryExceptions = safeExceptions;
        }

        return sentryEvent;
    }
}
