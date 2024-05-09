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
            MetroWindow window = new()
            {
                Width = 800,
                Height = 600,
                Title = operationModel.Operator!.Name,
                ResizeMode = ResizeMode.CanResizeWithGrip,
                TitleCharacterCasing = CharacterCasing.Normal,
                Content = new IOConnectorView() { DataContext = new IOConnectorViewModel(operationModel) }
            };
            //var iOConnectorViewModel =new IOConnectorViewModel(operationModel);
            //window.Content = new IOConnectorView() { DataContext = iOConnectorViewModel };
            window.ShowDialog();
        }

    }
}
