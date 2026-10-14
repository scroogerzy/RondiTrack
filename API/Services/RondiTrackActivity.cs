using System.Diagnostics;

namespace RondiTrack.Services;

public static class RondiTrackActivity
{
    public static readonly ActivitySource Source =
        new("ronditrack-api");
}
