using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VisionProcess.Common;
using VisionProcess.Lang.Strings;
using VisionProcess.ViewModel.ViewServices;
using VisionProcess.wpf.Views.Dialogs;

namespace VisionProcess.wpf.Services;


internal sealed class DialogService : IDialogService
{
    public IDisposable ShowProgressBarDialog(string dialogIdentifier = ViewServiceIdentifiers.RootDialogHost)
    {
        var identifier = NormalizeIdentifier(dialogIdentifier);

        InvokeOnUi(() =>
        {
            var progressDialog = new ProgressDialog();
            var dialogSession = DialogHost.GetDialogSession(identifier);

            if (dialogSession is not null)
            {
                dialogSession.UpdateContent(progressDialog);
            }
            else
            {
                _ = DialogHost.Show(progressDialog, identifier);
            }
        });

        return new DeferredScope(() => Close(identifier));
    }

    public void Close(string dialogIdentifier = ViewServiceIdentifiers.RootDialogHost)
    {
        var identifier = NormalizeIdentifier(dialogIdentifier);
        InvokeOnUi(() => CloseCurrentDialog(identifier));
    }

    public Task ShowOrReplaceMessageDialogAsync(
        string message,
        string header = "",
        string dialogIdentifier = ViewServiceIdentifiers.RootDialogHost)
    {
        return ShowMessageAsync(header, message, dialogIdentifier);
    }

    public async Task<bool> ShowOrReplaceMessageDialogWithCancelAsync(
        string message,
        string header = "",
        string dialogIdentifier = ViewServiceIdentifiers.RootDialogHost)
    {
        var result = await ShowMessageAsync(header, message, dialogIdentifier, MessageDialogButton.OKCancel);
        return result is string resultString && resultString == bool.TrueString;
    }

    public async Task<bool> ShowExitConfirmation(string dialogIdentifier = ViewServiceIdentifiers.RootDialogHost)
    {
        var header = Strings.ConfirmExitTitle;
        var message = Strings.ConfirmExitMessage;
        var buttonType = MessageDialogButton.OKCancel;
        var width = 350;
        var okButtonContent = Strings.Confirm;

        MessageDialog messageDialog = new(header, message, buttonType);
        messageDialog.Width = width;
        messageDialog.SetOKButtonContent(okButtonContent);
        var result = await ShowReplacingCurrentAsync(() => messageDialog, dialogIdentifier);

        return result is string resultString && resultString == bool.TrueString;
    }

    private static Task<object?> ShowMessageAsync(string header, string message, string dialogIdentifier, MessageDialogButton buttonType = MessageDialogButton.OK)
    {
        MessageDialog messageDialog = new(header, message, buttonType);

        return ShowReplacingCurrentAsync(() => messageDialog, dialogIdentifier);
    }

    private static Task<object?> ShowReplacingCurrentAsync(Func<object> dialogFactory, string dialogIdentifier)
    {
        var identifier = NormalizeIdentifier(dialogIdentifier);

        return InvokeOnUiAsync(async () =>
        {
            CloseCurrentDialog(identifier);
            await Task.Yield();
            return await DialogHost.Show(dialogFactory(), identifier);
        });
    }

    private static void CloseCurrentDialog(string identifier)
    {
        var dialogSession = DialogHost.GetDialogSession(identifier);
        dialogSession?.Close();
    }

    private static string NormalizeIdentifier(string? dialogIdentifier)
    {
        return string.IsNullOrWhiteSpace(dialogIdentifier)
            ? ViewServiceIdentifiers.RootDialogHost
            : dialogIdentifier;
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

    private static Task<T> InvokeOnUiAsync<T>(Func<Task<T>> action)
    {
        var dispatcher = Application.Current?.Dispatcher;
        if (dispatcher is null || dispatcher.CheckAccess())
            return action();

        return dispatcher.InvokeAsync(action).Task.Unwrap();
    }
}
