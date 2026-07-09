using System.Windows;
using System.Windows.Controls;

namespace VisionProcess.wpf.Views.Dialogs;

public enum MessageDialogButton
{
    //
    // 摘要:
    //     The message box displays an OK button.
    OK = 0,

    //
    // 摘要:
    //     The message box displays OK and Cancel buttons.
    OKCancel = 1,
}

/// <summary>
/// MessageDialog.xaml 的交互逻辑
/// </summary>
public partial class MessageDialog : UserControl
{
    public MessageDialog(string header, string message, MessageDialogButton buttonType = MessageDialogButton.OK)
    {
        InitializeComponent();
        HeaderTextBlock.Text = header;
        MessageTextBlock.Text = message;
        if (buttonType == MessageDialogButton.OKCancel)
        {
            SetMessageDialogButton(buttonType);
        }
    }

    public void SetHeight(double height)
    {
        Height = height;
    }

    public void SetWidth(double width)
    {
        Width = width;
    }

    public void SetButtonContent(string okButtonContent, string cancelButtonContent)
    {
        OkButton.Content = okButtonContent;
        CancelButton.Content = cancelButtonContent;
    }

    public void SetOKButtonContent(string okButtonContent)
    {
        OkButton.Content = okButtonContent;
    }

    public void SetCancelButtonContent(string cancelButtonContent)
    {
        CancelButton.Content = cancelButtonContent;
    }

    public void SetMessageDialogButton(MessageDialogButton messageDialogButton)
    {
        if (messageDialogButton == MessageDialogButton.OKCancel)
        {
            ButtonStackPanel.HorizontalAlignment = HorizontalAlignment.Right;
            OkButton.IsCancel = false;
            OkButton.IsDefault = true;
            CancelButton.IsCancel = true;
            OkButton.Style = (Style)FindResource("MaterialDesignFlatSecondaryButton");
            OkButton.Margin = new Thickness(0, 0, 20, 0);
            CancelButton.Visibility = Visibility.Visible;
        }
        else if (messageDialogButton == MessageDialogButton.OK)
        {
            ButtonStackPanel.HorizontalAlignment = HorizontalAlignment.Center;
            CancelButton.Visibility = Visibility.Collapsed;
            CancelButton.IsCancel = false;
            OkButton.IsCancel = true;
            OkButton.Style = null;
        }
    }
}
