namespace Client;

public partial class App
{
    private static int PromptUserForToolSelection(McpTools tools)
    {
        return tools.GetSelectionFromUser(static () => Console.ReadLine() ?? string.Empty, Console.Write);
    }

    private static void PromptUserForPropertyValues(McpToolSchema schema)
    {
        schema.GetPropertyValuesFromUser(static () => Console.ReadLine() ?? string.Empty, Console.Write);
    }

    private static void DisplayToolResponse(CallToolResponse response)
    {
        Console.Write(McpTools.ToolExecutionResponseHeader);
        foreach (var content in response.GetTextContent())
        {
            Console.WriteLine(content.Text);
        }
    }
}
