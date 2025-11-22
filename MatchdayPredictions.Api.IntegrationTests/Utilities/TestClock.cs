namespace MatchdayPredictions.Api.IntegrationTests.Utilities;

public static class TestClock
{
    public static DateTime TruncateToSecond(DateTime value)
        => value.AddTicks(-(value.Ticks % TimeSpan.TicksPerSecond));
}
