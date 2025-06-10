namespace Client.Tool;

public class McpToolSchema
{
    public string                Title       { get; set; } = string.Empty;
    public string                Description { get; set; } = string.Empty;
    public string                Type        { get; set; } = "object";
    public List<McpToolProperty> Properties  { get; set; } = [];

    public IReadOnlyDictionary<string, object?> GetCallToolArguments()
    {
        return Properties.ToDictionary(static p => p.Name, static p => p.Value);
    }
}
