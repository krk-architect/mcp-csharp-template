using Spectre.Console;

namespace Client;

public partial class App
{
    private static int PromptUserForToolSelection(McpTools tools)
    {
        // Create a cool banner
        AnsiConsole.Write(new FigletText("MCP Client").LeftJustified()
                                                      .Color(Color.Blue));

        // Create a styled panel for available tools
        var toolsTable = new Table().Border(TableBorder.Rounded)
                                    .BorderColor(Color.Grey)
                                    .AddColumn(new TableColumn("[bold]#[/]").Centered())
                                    .AddColumn(new TableColumn("[bold]Tool Name[/]").LeftAligned())
                                    .AddColumn(new TableColumn("[bold]Description[/]").LeftAligned());

        // Add exit option
        toolsTable.AddRow("[red]0[/]", "[red]Exit[/]", "[grey]Quit the application[/]");

        // Add tools with colors
        string[] colors =
        [
            "green"
          , "yellow"
          , "cyan"
          , "magenta"
          , "orange1"
        ];

        for (var i = 0; i < tools.Count; i++)
        {
            var tool  = tools[i];
            var color = colors[i % colors.Length];

            toolsTable.AddRow($"[{color}]{i + 1}[/]", $"[{color}]{tool.Name}[/]", $"[grey]{tool.Description}[/]");
        }

        AnsiConsole.Write(toolsTable);

        // Custom prompt using Spectre.Console's Ask method
        var selection = -1;
        while (selection < 0 || selection > tools.Count)
        {
            var input = AnsiConsole.Ask<string>("[bold blue]Select a tool:[/]");

            if (int.TryParse(input, out var choice) && choice >= 0 && choice <= tools.Count)
            {
                selection = choice;

                // Custom confirmation message
                var toolName = choice == 0
                                   ? "[red]Exit[/]"
                                   : $"[bold]{tools[choice - 1].Name}[/]";

                AnsiConsole.MarkupLine($"[blue]Selected Tool:[/] {toolName}");
            }
            else
            {
                AnsiConsole.MarkupLine($"[red]Please enter a number between 0 and {tools.Count}![/]");
            }
        }

        return selection;
    }

    private static void PromptUserForPropertyValues(McpToolSchema schema)
    {
        if (schema.Properties.Count <= 0)
        {
            return;
        }

        foreach (var property in schema.Properties)
        {
            if (property.Required)
            {
                string value;
                do
                {
                    // Use Spectre.Console's Markup.Remove to get clean text for Console.Write
                    var promptMarkup = $"[red]*[/] [bold]{property.Name}[/] [grey]({property.Type})[/]: ";

                    // First render the markup to show colors
                    AnsiConsole.Markup(promptMarkup);

                    // Then read input normally
                    value = Console.ReadLine() ?? string.Empty;

                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        break;
                    }

                    AnsiConsole.MarkupLine($"[red]{property.Name} is required! Please provide a value.[/]");
                } while (true);

                property.GetValueFromUser(() => value, static _ => { });
            }
            else
            {
                var promptMarkup = $"[bold]{property.Name}[/] [grey]({property.Type}, optional)[/]: ";

                // Render markup and read input on same line
                AnsiConsole.Markup(promptMarkup);
                var value = Console.ReadLine() ?? string.Empty;

                if (!string.IsNullOrWhiteSpace(value))
                {
                    property.GetValueFromUser(() => value, static _ => { });
                }
            }
        }
    }

    private static void DisplayToolResponse(CallToolResponse response)
    {
        AnsiConsole.Write(new Rule("[bold green]Tool Response[/]")
                          {
                              Style = Style.Parse("green")
                          });

        foreach (var content in response.GetTextContent())
        {
            // Remove the header entirely to avoid truncation
            var panel = new Panel(content.Text ?? string.Empty)
                       .Border(BoxBorder.Rounded)
                       .BorderColor(Color.Green);

            // No .Header() call because it was getting truncated with shorter responses

            AnsiConsole.Write(panel);
        }

        AnsiConsole.WriteLine();
    }
}
