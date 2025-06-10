// ReSharper disable InvertIf

namespace Test.Unit;

/// <summary>
///     This class will have all of the One Time setup/teardown logic applicable to all the unit tests.<br />
///     It currently does nothing but write output.
/// </summary>
[SetUpFixture]
public class UnitTests
{
    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        WriteLine(MessageDestination.TestContext, "OneTimeSetup");
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        WriteLine(MessageDestination.TestContext, "OneTimeTearDown");
    }

    public static void WriteLine(MessageDestination destination, string message, string prefix = "", bool flush = false)
    {
        var formattedMessage = $"[{DateTime.Now:HH:mm:ss.fff}] {prefix}{message}";

        if (destination.HasFlag(MessageDestination.Console))
        {
            Console.WriteLine(formattedMessage);

            if (flush)
            {
                Console.Out.Flush();
            }
        }

        if (destination.HasFlag(MessageDestination.Debug))
        {
            Debug.WriteLine(formattedMessage);

            if (flush)
            {
                Debug.Flush();
            }
        }

        if (destination.HasFlag(MessageDestination.TestContext))
        {
            TestContext.WriteLine(formattedMessage);

            if (flush)
            {
                TestContext.Out.Flush();
            }
        }
    }
}

[Flags]
public enum MessageDestination
{
    Console     = 1
  , Debug       = 2
  , TestContext = 4
}
