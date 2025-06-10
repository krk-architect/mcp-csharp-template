namespace StdioServer.Tools;

[TestFixture]
public class PingPongToolTests
{
    [TestCase("ping", PingPongTool.PongResponse              )]
    [TestCase("PING", PingPongTool.NotAPingPongMasterResponse)]
    [TestCase("pong", PingPongTool.NotAPingPongMasterResponse)]
    [TestCase("a b" , PingPongTool.NotAPingPongMasterResponse)]
    [TestCase(""    , PingPongTool.NotAPingPongMasterResponse)]
    public void PingPong_ShouldReturnExpectedResponse(string input, string expected)
    {
        // Arrange

        // Act
        var result = PingPongTool.PingPong(input);

        // Assert
        Assert.That(result, Is.EqualTo(expected), "The PingPong tool should return the correct response.");
    }
}
