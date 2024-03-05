using MahApps.Metro.Controls;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using VisionProcess.Controls.UserControls;
using VisionProcess.Models;
using VisionProcess.Views;
using VisionProcess.ViewModels;

namespace VisionProcess.Services
{
    public static class WindowDialogService
    {

        public static void OpenIOConnectorDialog(OperationModel operationModel)
        {
            MetroWindow window = new MetroWindow();
            window.Width = 800;
            window.Height = 600;
            window.Title = operationModel.Operator!.Name;
            window.ResizeMode = ResizeMode.CanResizeWithGrip;
            window.TitleCharacterCasing = CharacterCasing.Normal;
            var iOConnectorViewModel =new IOConnectorViewModel(operationModel);
            window.Content = new IOConnectorView() { DataContext = iOConnectorViewModel };
            window.ShowDialog();
        }

    }
}
