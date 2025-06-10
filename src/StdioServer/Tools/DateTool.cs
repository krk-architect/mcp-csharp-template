namespace StdioServer.Tools;

[McpServerToolType]
public static class DateTool
{
    [McpServerTool]
    [Description("Responds with the current date")]
    public static string GetDate() => SystemDateTime.Now.ToString("dddd MMMM d, yyyy");
}
