namespace Server.Tools;

[McpServerToolType]
public static class PingPongTool
{
    [McpServerTool]
    [Description("Answers 'ping' with 'pong'; otherwise says 'clearly you are not a ping pong master'.")]
    public static string PingPong(string message) => message == "ping"
                                                         ? "pong"
                                                         : "clearly you are not a ping pong master";
}
