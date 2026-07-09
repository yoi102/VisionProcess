using AvalonDock;
using AvalonDock.Themes;
using CommunityToolkit.Mvvm.DependencyInjection;
using MessagePipe;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using VisionProcess.Common.Events;
using VisionProcess.ViewModel.ViewModels;
using VisionProcess.ViewModel.ViewServices;

namespace VisionProcess.wpf.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView
    {
        public MainView(MainViewModel mainViewModel, ISubscriber<ThemeChangedEvent> subscriber)
        {
            InitializeComponent();

            this.DataContext = mainViewModel;

            subscriber.Subscribe((h) =>
            {
                if (h.IsDark)
                {
                    DockingManager.Theme = new ArcDarkTheme();
                }
                else
                {
                    DockingManager.Theme = new ArcLightTheme();
                }
            });
            Closing += OnWindowClosing;
        }

        private void OnWindowClosing(object? sender, CancelEventArgs e)
        {
            e.Cancel = true; // 临时取消关闭

            // 不能直接 await，所以用 async void 包装异步处理
            HandleExitConfirmationAsync(e);
        }

        private async void HandleExitConfirmationAsync(CancelEventArgs e)
        {
            var dialog = Ioc.Default.GetRequiredService<IDialogService>();

            bool confirm = await dialog.ShowExitConfirmation();

            if (confirm)
            {
                // 手动移除关闭事件，避免递归触发
                Closing -= OnWindowClosing;

                Close();  // 程序关闭
            }
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