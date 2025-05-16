namespace Test.Unit.ServerTests;

[TestFixture]
public class DateToolTests
{
    [Test]
    public void GetDate_ShouldReturnCurrentDateFormatted()
    {
        // Arrange
        var now = DateTime.Now;
        var expected = now.ToString("dddd MMMM d, yyyy");

        // Act
        using var guard  = new SystemDateTimeGuard(() => now);
        var       result = DateTool.GetDate();

        // Assert
        Assert.That(result, Is.EqualTo(expected), "The DateTool should return the current date formatted as 'dddd MMMM d, yyyy'.");
    }
}
