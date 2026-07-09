using System;
using System.Collections.Generic;
using System.Text;

namespace VisionProcess.Common;

public sealed class DeferredScope(Action onDispose) : IDisposable
{
    private readonly Action _onDispose = onDispose ?? throw new ArgumentNullException(nameof(onDispose));
    private bool _disposed;

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;

        _onDispose.Invoke();
    }
}
