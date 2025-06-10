namespace Client.Extensions;

public static partial class Extensions
{
    public static IEnumerable<Content> GetTextContent(this CallToolResponse @this)
    {
        return @this.Content.Where(static c => c.Type == "text");
    }
}
