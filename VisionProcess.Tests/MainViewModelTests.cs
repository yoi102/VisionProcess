using System;
using System.Linq;
using VisionProcess.Abstractions.ToolBase;
using VisionProcess.Tools.Services;
using VisionProcess.ViewModel.Tooling;
using VisionProcess.ViewModel.ViewModels;
using VisionProcess.ViewModel.ViewModels.Toolboxes;
using VisionProcess.ViewModel.ViewServices;
using Xunit;

namespace VisionProcess.Tests
{
    public class MainViewModelTests
    {
        public MainViewModelTests()
        {
            TestIoc.EnsureConfigured();
        }

        private static MainViewModel CreateMainViewModel(
            IWindowDialogService? windowDialogService = null,
            IOperatorFactory? operatorFactory = null,
            IOperatorViewModelFactory? viewModelFactory = null)
        {
            var folderExplorer = new FolderExplorerToolboxViewModel(new StubFileDocumentService());
            var properties = new PropertiesToolboxViewModel(new StubToolboxIconsService());
            var editorFactory = new EditorViewModelFactory(
                windowDialogService ?? new StubWindowDialogService(),
                operatorFactory ?? new OperatorFactory(),
                viewModelFactory ?? new OperatorViewModelFactory(new StubFileDialogService()));

            return new MainViewModel(
                new StubCultureSettingService(),
                new StubThemeSettingService(),
                editorFactory,
                folderExplorer,
                properties);
        }

        [Fact]
        public void CloseEditor_WhenSelectedEditorIsClosed_SelectsAnotherEditor()
        {
            // Arrange
            var mainViewModel = CreateMainViewModel();
            mainViewModel.AddEditorCommand.Execute(null);
            var firstEditor = mainViewModel.Editors[0];
            var secondEditor = mainViewModel.Editors[1];
            mainViewModel.SelectedEditor = firstEditor;

            // Act
            mainViewModel.CloseEditorCommand.Execute(firstEditor.Id);

            // Assert
            Assert.DoesNotContain(firstEditor, mainViewModel.Editors);
            Assert.Equal(secondEditor, mainViewModel.SelectedEditor);
        }

        [Fact]
        public void CloseEditor_WhenOnlyEditorIsClosed_SelectedEditorBecomesNull()
        {
            // Arrange
            var mainViewModel = CreateMainViewModel();
            var firstEditor = mainViewModel.Editors[0];
            mainViewModel.SelectedEditor = firstEditor;

            // Act
            mainViewModel.CloseEditorCommand.Execute(firstEditor.Id);

            // Assert
            Assert.Empty(mainViewModel.Editors);
            Assert.Null(mainViewModel.SelectedEditor);
        }
    }
}
