namespace Server.Tools;

[McpServerToolType]
public static class DateTool
{
    [McpServerTool]
    [Description("Responds with the current date")]
    public static string GetDate() => DateTime.Now.ToString("dddd MMMM d, yyyy");
}
