namespace Client.Extensions;

public static partial class Extensions
{
    public static async ValueTask<McpTools> GetMcpToolsAsync(this IMcpClient @this)
    {
        return new (await @this.ListToolsAsync().ConfigureAwait(false));
    }
}
