namespace StdioServer;

public static class SystemDateTime
{
    internal static Func<DateTime> TimeProvider { get; set; } = static () => DateTime.UtcNow;

    public static DateTime Now
    {
        get
        {
            var value = TimeProvider();

            return value.Kind == DateTimeKind.Utc ? value.ToLocalTime() : value;
        }
    }

    public static DateTime UtcNow => TimeProvider();
}

public sealed class SystemDateTimeGuard : Guard<Func<DateTime>>
{
    public SystemDateTimeGuard(Func<DateTime> timeProvider)
        : base(SystemDateTime.TimeProvider)
    {
        SystemDateTime.TimeProvider = () =>
                                      {
                                          var value = timeProvider();
                                          return value.Kind == DateTimeKind.Local ? value.ToUniversalTime() : value;
                                      };
    }

    protected override void Restore() => SystemDateTime.TimeProvider = OriginalValue;
}
