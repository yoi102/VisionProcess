using Antelcat.I18N.WPF;
using AvalonDock;
using AvalonDock.DependencyInjection;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using VisionProcess.Abstractions.Services;
using VisionProcess.Abstractions.ToolBase;
using VisionProcess.Persistence;
using VisionProcess.Persistence.Migrations;
using VisionProcess.Runtime.Models;
using VisionProcess.Runtime.Services;
using VisionProcess.ViewModel;
using VisionProcess.Tools.Services;
using VisionProcess.ViewModel.Tooling;
using VisionProcess.ViewModel.ViewModels;
using VisionProcess.ViewModel.ViewModels.Toolboxes;
using VisionProcess.ViewModel.ViewServices;
using VisionProcess.wpf.Services;
using VisionProcess.wpf.Views;
using VisionProcess.wpf.ViewServices;

namespace VisionProcess.wpf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application, IFileDocumentService
    {
        private const string DocumentFileExtension = ".vproc";
        private const string DocumentFileFilter = "VisionProcess documents (*.vproc)|*.vproc|All files (*.*)|*.*";
        private static Mutex? appMutex;

        public App()
        {
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;

            string lang = System.Globalization.CultureInfo.CurrentCulture.Name;
            var culture = new System.Globalization.CultureInfo(lang);
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
            I18NExtension.Culture = culture;
            Services = ConfigureServices();
            Ioc.Default.ConfigureServices(Services);
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets the current <see cref="App"/> instance in use
        /// </summary>
        public new static App Current => (App)System.Windows.Application.Current;

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
        /// </summary>
        public IServiceProvider Services { get; }

        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("User32.dll", CharSet = CharSet.Unicode, EntryPoint = "FlashWindow")]
        public static extern void FlashWindow(IntPtr hwnd, bool bInvert);

        [DllImport("User32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);



        protected override void OnStartup(StartupEventArgs e)
        {
            CheckMutex(e);
        }

        /// <summary>
        /// 保存单个文档。若文档已有 <see cref="EditorViewModel.FilePath"/> 则直接覆盖，否则弹出另存为对话框。
        /// </summary>
        public void SaveDocument(EditorViewModel document)
        {
            if (!string.IsNullOrEmpty(document.FilePath))
            {
                SaveDocumentToPath(document, document.FilePath);
                return;
            }

            SaveDocumentAs(document);
        }

        /// <summary>
        /// 始终弹出保存对话框，将文档另存为新文件。
        /// </summary>
        public void SaveDocumentAs(EditorViewModel document)
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Save As",
                DefaultExt = DocumentFileExtension,
                Filter = DocumentFileFilter,
                FileName = GetDocumentFileName(document)
            };

            if (dialog.ShowDialog() != true)
            {
                return;
            }

            SaveDocumentToPath(document, dialog.FileName);
            document.FilePath = dialog.FileName;
        }

        /// <summary>
        /// 从用户选择的文件加载一个文档。
        /// </summary>
        public EditorViewModel? LoadDocument()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Load Document",
                DefaultExt = DocumentFileExtension,
                Filter = DocumentFileFilter
            };

            if (dialog.ShowDialog() != true)
            {
                return null;
            }

            var persistence = Services.GetRequiredService<ProcessPersistenceService>();
            var container = persistence.LoadFromFile(dialog.FileName);
            var editorDto = container?.MainView?.Editors.FirstOrDefault();
            if (editorDto is null)
            {
                return null;
            }

            var editor = CreateEditorFromDto(editorDto);
            editor.FilePath = dialog.FileName;
            return editor;
        }

        private static string GetDocumentFileName(EditorViewModel document)
        {
            if (string.IsNullOrEmpty(document.FilePath))
            {
                return $"{document.Name}{DocumentFileExtension}";
            }

            return Path.GetFileName(document.FilePath);
        }

        private static void SaveDocumentToPath(EditorViewModel document, string filePath)
        {
            var persistence = Current.Services.GetRequiredService<ProcessPersistenceService>();
            var dto = new EditorDto
            {
                Id = document.Id,
                Name = document.Name,
                Process = ProcessMapper.ToDto(document.Process)
            };
            var container = new VersionedContainer
            {
                MainView = new MainViewDto
                {
                    AutoSelectNewEditor = false,
                    SelectedEditorId = dto.Id,
                    Editors = [dto]
                }
            };

            persistence.SaveToFile(container, filePath);
        }

        private EditorViewModel CreateEditorFromDto(EditorDto editorDto)
        {
            var dialogService = Services.GetRequiredService<IWindowDialogService>();
            var process = editorDto.Process is null ? new ProcessModel() : ProcessMapper.ToModel(editorDto.Process);
            var editor = new EditorViewModel(process, editorDto.Id, editorDto.Name)
            {
                Parent = null
            };
            editor.Process.OperationsMenu = editor.OperationsMenu;
            foreach (var operation in editor.Process.Operations)
            {
                operation.WindowDialogService = dialogService;
            }

            return editor;
        }

        /// <summary>
        /// Configures the services for the application.
        /// </summary>
        private static IServiceProvider ConfigureServices()
        {
            var serializer = new ProcessSerializer(new MigrationRunner(Array.Empty<IMigration>()));
            var persistence = new ProcessPersistenceService(serializer);

            var services = new ServiceCollection();
            var dialogService = new WindowDialogService();

            services.AddMessagePipe();
            services.AddViewModels();
            services.AddTransient<ICultureSettingService, CultureSettingService>();
            services.AddTransient<IThemeSettingService, ThemeSettingService>();
            services.AddTransient<IDialogService, DialogService>();
            services.AddTransient<ISnackbarService, SnackbarService>();
            services.AddTransient<IWindowDialogService, WindowDialogService>();
            services.AddTransient<IFileDialogService, FileDialogService>();
            services.AddTransient<IOperatorFactory, OperatorFactory>();
            services.AddTransient<IOperatorViewModelFactory, OperatorViewModelFactory>();
            services.AddTransient<IToolboxIconsService, ToolboxIconsService>();
            services.AddSingleton(serializer);
            services.AddSingleton(persistence);
            services.AddSingleton<IFileDocumentService>(_ => Current);
            services.AddTransient<EditorViewModel>();


            services.AddSingleton<MainView>();

            return services.BuildServiceProvider();
        }

        [DllImport("User32.dll")]
        private static extern int ShowWindow(IntPtr hwnd, uint nCmdShow);

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            e.Handled = true;
            try
            {
                var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"configs\exit-error.log");
                File.AppendAllText(logPath, $"{DateTime.Now:O} Unhandled exception: {e.Exception}\n");
            }
            catch
            {
                // ignore logging failures
            }

            Shutdown();
        }

        private void CheckMutex(StartupEventArgs e)
        {
            Process currentProc = Process.GetCurrentProcess();
            appMutex = new System.Threading.Mutex(true, currentProc.ProcessName, out bool createdNew);
            if (createdNew)
            {
                base.OnStartup(e);

                var mainWindow = Current.Services.GetRequiredService<MainView>();
                this.MainWindow = mainWindow;
                mainWindow.Show();
            }
            else
            {
                var proc = Process.GetProcessesByName(currentProc.ProcessName)
                                         .FirstOrDefault(x => x.Id != currentProc.Id);
                if (proc != null)
                {
                    IntPtr handle = proc.MainWindowHandle;
                    ShowWindow(handle, 9);
                    SetForegroundWindow(handle);
                }

                Environment.Exit(0);
            }
        }
    }
}
