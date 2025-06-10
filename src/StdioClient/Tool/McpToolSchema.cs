namespace StdioClient.Tool;

public class McpToolSchema
{
    public string                Title       { get; set; } = string.Empty;
    public string                Description { get; set; } = string.Empty;
    public string                Type        { get; set; } = "object";
    public List<McpToolProperty> Properties  { get; set; } = [];

    public void GetPropertyValuesFromUser(Func<string> inputFunc, Action<string> outputAction)
    {
        if (Properties.Count <= 0)
        {
            return;
        }

        outputAction("""

                     Enter input values:
                     ===================

                     """);

        foreach (var property in Properties)
        {
            property.GetValueFromUser(inputFunc, outputAction);
        }
    }

    public IReadOnlyDictionary<string, object?> GetCallToolArguments()
    {
        return Properties.ToDictionary(static p => p.Name, static p => p.Value);
    }
}
