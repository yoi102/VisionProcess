using System;
using System.Linq;
using VisionProcess.ViewModel.ViewModels;
using Xunit;

namespace VisionProcess.Tests
{
    public class MainViewModelTests
    {
        [Fact]
        public void CloseEditor_WhenSelectedEditorIsClosed_SelectsAnotherEditor()
        {
            // Arrange
            var mainViewModel = new MainViewModel((Runtime.Services.IDialogService?)null);
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
            var mainViewModel = new MainViewModel((Runtime.Services.IDialogService?)null);
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
