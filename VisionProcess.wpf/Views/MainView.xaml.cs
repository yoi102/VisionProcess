using AvalonDock;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics;
using System.Windows;
using VisionProcess.Application.ViewModels;

namespace VisionProcess.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView
    {
        public MainView()
        {
            InitializeComponent();

            this.DataContext = App.Current.Services.GetService<MainViewModel>();
        }

        private void DockingManager_DocumentClosing(object sender, DocumentClosingEventArgs e)
        {
            if (e.Document.Content is EditorViewModel editor &&
                DataContext is MainViewModel mainViewModel)
            {
                mainViewModel.CloseEditorCommand.Execute(editor.Id);
                e.Cancel = true;
            }
        }

        private void IconClicked(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo("https://github.com/yoi102/VisionProcess") { UseShellExecute = true });
        }
    }
}