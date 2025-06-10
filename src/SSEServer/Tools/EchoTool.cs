namespace SSEServer.Tools;

[McpServerToolType]
public static class EchoTool
{
    [McpServerTool]
    [Description("Answers 'echo' with whatever the user said after it.")]
    public static string Echo(string message) => message;
}
