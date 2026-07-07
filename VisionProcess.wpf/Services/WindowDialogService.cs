using MahApps.Metro.Controls;
using System.Windows;
using System.Windows.Controls;
using VisionProcess.Application.ViewModels;
using VisionProcess.Runtime.Models;
using VisionProcess.Runtime.Services;
using VisionProcess.Views;

namespace VisionProcess.Services
{
    /// <summary>
    /// WPF 对话框服务的实现，将 Application 层的 <see cref="IDialogService"/> 映射到具体窗口。
    /// </summary>
    public sealed class WindowDialogService : IDialogService
    {
        public void OpenIOConnectorDialog(OperationModel operationModel)
        {
            MetroWindow window = new()
            {
                Width = 800,
                Height = 600,
                Title = operationModel.Operator!.Name,
                ResizeMode = ResizeMode.CanResizeWithGrip,
                TitleCharacterCasing = CharacterCasing.Normal,
                Content = new IOConnectorView() { DataContext = new IOConnectorViewModel(operationModel) }
            };
            window.ShowDialog();
        }
    }
}
