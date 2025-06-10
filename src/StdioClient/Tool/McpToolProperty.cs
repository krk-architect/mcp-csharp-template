namespace StdioClient.Tool;

public class McpToolProperty
{
    public string Name     { get; init; } = null!;
    public string Type     { get; set;  } = null!;
    public bool   Required { get; set;  }

    public object? Value { get; private set;  }

    public void GetValueFromUser(Func<string> inputFunc, Action<string> outputAction)
    {
        do
        {
            outputAction($"{Name} [{(Required ? "Required" : "Optional")}]: ");

            var value = inputFunc();

            if (!string.IsNullOrWhiteSpace(value))
            {
                Value = Type switch
                        {
                            "string"  => value
                          , "number"  => double.TryParse(value, out var numberValue) ? numberValue : null
                          , "boolean" => bool.TryParse(value, out var boolValue) ? boolValue : null
                          , _         => value
                        };

                break;
            }

            if (!Required)
            {
                break;
            }

            outputAction($"The parameter '{Name}' is required. Please provide a value.{Environment.NewLine}");
        } while (true);
    }
}
