using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using VisionProcess.Abstractions.Services;
using VisionProcess.Abstractions.ToolBase;
using VisionProcess.Runtime.Models;
using VisionProcess.ViewModel;
using VisionProcess.Tools.Services;
using VisionProcess.ViewModel.Tooling;
using VisionProcess.ViewModel.ViewModels;
using VisionProcess.ViewModel.ViewModels.Toolboxes;
using VisionProcess.ViewModel.ViewServices;

namespace VisionProcess.Tests
{
    internal sealed class StubCultureSettingService : ICultureSettingService
    {
        public int CurrentLCID { get; private set; } = 1033;

        public void ChangeCulture(string language) { }

        public void ChangeCulture(int lcid) => CurrentLCID = lcid;

        public int GetCurrentCultureLCID() => CurrentLCID;
    }

    internal sealed class StubThemeSettingService : IThemeSettingService
    {
        public bool IsDarkTheme { get; private set; }

        public void ToggleThemeLightDark() => IsDarkTheme = !IsDarkTheme;

        public void ApplyThemeLightDark(bool isDarkTheme) => IsDarkTheme = isDarkTheme;
    }

    internal sealed class StubDialogService : IDialogService
    {
        public void Close(string dialogIdentifier = ViewServiceIdentifiers.RootDialogHost) { }

        public Task ShowOrReplaceMessageDialogAsync(string message, string header = "", string dialogIdentifier = ViewServiceIdentifiers.RootDialogHost)
            => Task.CompletedTask;

        public Task<bool> ShowOrReplaceMessageDialogWithCancelAsync(string message, string header = "", string dialogIdentifier = ViewServiceIdentifiers.RootDialogHost)
            => Task.FromResult(true);

        public IDisposable ShowProgressBarDialog(string dialogIdentifier = ViewServiceIdentifiers.RootDialogHost)
            => new NullDisposable();

        public Task<bool> ShowExitConfirmation(string dialogIdentifier = ViewServiceIdentifiers.RootDialogHost)
            => Task.FromResult(true);
    }

    internal sealed class StubSnackbarService : ISnackbarService
    {
        public void Enqueue(object content, TimeSpan? durationOverride = null, bool promote = false, bool neverConsiderToBeDuplicate = false) { }

        public void EnqueueInAll(object content, TimeSpan? durationOverride = null, bool promote = false, bool neverConsiderToBeDuplicate = false) { }

        public void Enqueue(object identifier, object content, TimeSpan? durationOverride = null, bool promote = false, bool neverConsiderToBeDuplicate = false) { }
    }

    internal sealed class StubWindowDialogService : IWindowDialogService
    {
        public OperationModel? OpenedOperation { get; private set; }

        public void OpenIOConnectorDialog(OperationModel operationModel)
            => OpenedOperation = operationModel;
    }

    internal sealed class StubFileDialogService : IFileDialogService
    {
        public IReadOnlyList<string>? PickImageFiles() => null;
    }

    internal sealed class StubFileDocumentService : IFileDocumentService
    {
        public EditorViewModel? LoadDocument() => null;

        public void SaveDocument(EditorViewModel document) { }

        public void SaveDocumentAs(EditorViewModel document) { }
    }

    internal sealed class StubToolboxIconsService : IToolboxIconsService
    {
        public object Explorer { get; } = new object();
        public object Layers { get; } = new object();
        public object Terminal { get; } = new object();
        public object Search { get; } = new object();
        public object Git { get; } = new object();
        public object Problems { get; } = new object();
    }

    internal sealed class NullDisposable : IDisposable
    {
        public void Dispose() { }
    }

    internal static class TestIoc
    {
        private static readonly object Lock = new object();
        private static bool configured;

        public static void EnsureConfigured()
        {
            lock (Lock)
            {
                if (configured)
                    return;

                var services = new ServiceCollection();
                services.AddViewModels();
                services.AddTransient<ICultureSettingService, StubCultureSettingService>();
                services.AddTransient<IThemeSettingService, StubThemeSettingService>();
                services.AddTransient<IDialogService, StubDialogService>();
                services.AddTransient<ISnackbarService, StubSnackbarService>();
                services.AddTransient<IWindowDialogService, StubWindowDialogService>();
                services.AddTransient<IFileDialogService, StubFileDialogService>();
                services.AddTransient<IOperatorFactory, OperatorFactory>();
                services.AddTransient<IOperatorViewModelFactory>(sp => new OperatorViewModelFactory(sp.GetRequiredService<IFileDialogService>()));
                services.AddTransient<IToolboxIconsService, StubToolboxIconsService>();
                services.AddTransient<IFileDocumentService, StubFileDocumentService>();
                services.AddTransient<FolderExplorerToolboxViewModel>();
                services.AddTransient<PropertiesToolboxViewModel>();

                Ioc.Default.ConfigureServices(services.BuildServiceProvider());
                configured = true;
            }
        }
    }
}
