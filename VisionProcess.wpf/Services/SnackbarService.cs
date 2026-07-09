using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using VisionProcess.ViewModel.ViewServices;
using VisionProcess.wpf.Helpers;

namespace VisionProcess.wpf.Services;


internal sealed class SnackbarService : ISnackbarService
{
    public void Enqueue(
        object content,
        TimeSpan? durationOverride = null,
        bool promote = false,
        bool neverConsiderToBeDuplicate = false)
    {
        Enqueue(ViewServiceIdentifiers.RootSnackbar, content, durationOverride, promote, neverConsiderToBeDuplicate);
    }

    public void EnqueueInAll(
        object content,
        TimeSpan? durationOverride = null,
        bool promote = false,
        bool neverConsiderToBeDuplicate = false)
    {
        InvokeOnUi(() =>
        {
            EnqueueCore(SnackbarIdentifierAssist.GetAllSnackbars(), content, durationOverride, promote, neverConsiderToBeDuplicate);
        });
    }

    public void Enqueue(
        object identifier,
        object content,
        TimeSpan? durationOverride = null,
        bool promote = false,
        bool neverConsiderToBeDuplicate = false)
    {
        ArgumentNullException.ThrowIfNull(identifier);

        InvokeOnUi(() =>
        {
            EnqueueCore(SnackbarIdentifierAssist.GetSnackbars(identifier), content, durationOverride, promote, neverConsiderToBeDuplicate);
        });
    }

    private static void EnqueueCore(
        IReadOnlyCollection<Snackbar> snackbars,
        object content,
        TimeSpan? durationOverride,
        bool promote,
        bool neverConsiderToBeDuplicate)
    {
        foreach (var snackbar in snackbars)
        {
            snackbar.MessageQueue?.Enqueue(
                content,
                null,
                null,
                null,
                promote,
                neverConsiderToBeDuplicate,
                durationOverride);
        }
    }

    private static void InvokeOnUi(Action action)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess())
        {
            action();
            return;
        }

        dispatcher.Invoke(action);
    }
}
