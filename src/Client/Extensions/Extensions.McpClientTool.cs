namespace Client.Extensions;

public static partial class McpToolParser
{
    public static McpToolSchema ParseToolSchema(this McpClientTool @this)
    {
        var jsonSchema = @this.JsonSchema;

        var toolSchema = new McpToolSchema();

        if (jsonSchema.TryGetProperty("title", out var titleElement))
        {
            toolSchema.Title = titleElement.GetString() ?? string.Empty;
        }

        if (jsonSchema.TryGetProperty("description", out var descriptionElement))
        {
            toolSchema.Description = descriptionElement.GetString() ?? string.Empty;
        }

        if (jsonSchema.TryGetProperty("type", out var typeElement))
        {
            toolSchema.Type = typeElement.GetString() ?? "object";
        }

        // Check if the schema contains a "properties" object which defines parameters
        if (!jsonSchema.TryGetProperty("properties", out var propertiesElement) || propertiesElement.ValueKind != JsonValueKind.Object)
        {
            return toolSchema;
        }

        var requiredProperties = new HashSet<string>();
        if (jsonSchema.TryGetProperty("required", out var requiredArray) && requiredArray.ValueKind == JsonValueKind.Array)
        {
            foreach (var requiredProperty in requiredArray.EnumerateArray())
            {
                if (requiredProperty.ValueKind != JsonValueKind.String)
                {
                    continue;
                }

                var requiredPropertyName = requiredProperty.GetString();
                if (requiredPropertyName is not null)
                {
                    requiredProperties.Add(requiredPropertyName);
                }
            }
        }

        foreach (var jsonProperty in propertiesElement.EnumerateObject())
        {
            var property = new McpToolProperty
                           {
                               Name = jsonProperty.Name
                           };

            if (jsonProperty.Value.TryGetProperty("type", out var propertyTypeElement))
            {
                property.Type = propertyTypeElement.GetString() ?? "string";
            }

            if (requiredProperties.Contains(property.Name))
            {
                property.Required = true;
            }

            toolSchema.Properties.Add(property);
        }

        toolSchema.Properties = [.. toolSchema.Properties.OrderBy(static p => p.Name)];

        return toolSchema;
    }
}
