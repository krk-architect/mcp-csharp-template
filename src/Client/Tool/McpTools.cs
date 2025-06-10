namespace Client.Tool;

public class McpTools
{
    public McpTools(IList<McpClientTool> tools)
    {
        Tools = tools as List<McpClientTool> ?? [.. tools];
    }

    private List<McpClientTool> Tools { get; }

    public int Count => Tools.Count;

    public McpClientTool this[int index] => Tools[index];
}
