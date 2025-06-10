namespace StdioClient.Tool;

public class McpTools
{
    public const string ToolExecutionResponseHeader = $"""

                                                       Response:{" "}
                                                       """;

    public McpTools(IList<McpClientTool> tools)
    {
        Tools = tools as List<McpClientTool> ?? [.. tools];
    }

    private List<McpClientTool> Tools { get; }

    public int Count => Tools.Count;

    public McpClientTool this[int index] => Tools[index];

    public int MaxNameLength => Tools.Max(static tool => tool.Name.Length);

    private string GetAvailableToolsForDisplay()
    {
        var sb = new StringBuilder();

        sb.Append("""

                  Available tools:
                  ================
                  0. Exit
                  """);

        var maxNameLength = MaxNameLength;

        for (var i = 0; i < Count; i++)
        {
            var tool = Tools[i];

            sb.Append($"""

                       {i + 1}. {tool.Name.PadRight(maxNameLength, ' ')} - {tool.Description}
                       """);
        }

        return sb.ToString();
    }

    public int GetSelectionFromUser(Func<string> inputFunc, Action<string> outputAction)
    {
        const string selectToolPrompt = $"""

                                         Select a tool to execute (enter the number):{" "}
                                         """;

        outputAction($"""
                      {GetAvailableToolsForDisplay()}
                      {selectToolPrompt}
                      """);

        var selection = -1;
        while (selection < 0 || selection > Count)
        {
            if (int.TryParse(inputFunc(), out var input))
            {
                selection = input;
                if (selection >= 0 && selection <= Count)
                {
                    continue;
                }

                outputAction($"""
                              Please enter a number between 0 and {Count}.
                              {selectToolPrompt}
                              """);
            }
            else
            {
                outputAction($"""
                              Please enter a valid number.
                              {selectToolPrompt}
                              """);
            }
        }

        return selection;
    }
}
