namespace HttpServer.Tools;

[McpServerToolType]
public static class PingPongTool
{
    public const string PongResponse               = "pong";
    public const string NotAPingPongMasterResponse = "clearly you are not a ping pong master";

    [McpServerTool]
    [Description("Answers 'ping' with 'pong'; otherwise says 'clearly you are not a ping pong master'.")]
    public static string PingPong(string message) => message == "ping"
                                                         ? PongResponse
                                                         : NotAPingPongMasterResponse;
}
