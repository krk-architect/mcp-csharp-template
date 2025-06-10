namespace Client.Extensions;

public static partial class Extensions
{
    public static Uri ToUri(this string @this)
    {
        return new (@this);
    }
}
