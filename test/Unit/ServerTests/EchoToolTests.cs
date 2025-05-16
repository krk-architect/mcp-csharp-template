namespace Test.Unit.ServerTests;

[TestFixture]
public class EchoToolTests
{
    [TestCase("Hello, World!")]
    [TestCase(" #1zz!@ $%^&*()_+|~`  ")]
    [TestCase(" ")]
    [TestCase("")]
    [TestCase("""

              blank lines

              more

              blank lines

              """)]
    public void Echo_ShouldReturnInputMessage(string input)
    {
        // Arrange

        // Act
        var result = EchoTool.Echo(input);

        // Assert
        Assert.That(result, Is.EqualTo(input), "The Echo tool should return the input message.");
    }
}
