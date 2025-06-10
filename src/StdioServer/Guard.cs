namespace StdioServer;

public abstract class Guard<T> : IDisposable
{
    protected Guard (T originalValue)
    {
        OriginalValue = originalValue;
    }

    protected T OriginalValue { get; }

    protected abstract void Restore();

    #region IDisposable

    private bool IsDisposed { get; set; }

    [ExcludeFromCodeCoverage]
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    [ExcludeFromCodeCoverage]
    ~Guard()
    {
        Dispose(false);
    }

    [ExcludeFromCodeCoverage]
    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed)
        {
            return;
        }

        IsDisposed = true;

        if (!disposing)
        {
            return;
        }

        Restore();
    }

    #endregion
}
