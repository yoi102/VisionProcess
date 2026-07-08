using System.Linq;
using System.Threading;
using System.Windows;
using VisionProcess.ViewModel.ViewModels;
using VisionProcess.Contracts;
using VisionProcess.Persistence;
using VisionProcess.Persistence.Migrations;
using VisionProcess.Runtime.Models;
using VisionProcess.Runtime.Services;
using VisionProcess.Tools.Tooling;
using Xunit;

namespace VisionProcess.Tests
{
    public class PersistenceRoundTripTests
    {
        [Fact]
        public void ProcessMapper_RoundTrip_PreservesOperationsAndConnections()
        {
            // Arrange
            var process = new ProcessModel();
            var editor = new EditorViewModel(process, default, "Test");

            var acquire = new OperationModel { Operator = OperatorFactory.Create("AcquireImage"), Location = new Point(10, 20) };
            var threshold = new OperationModel { Operator = OperatorFactory.Create("ImageThreshold"), Location = new Point(100, 200) };

            process.Operations.Add(acquire);
            process.Operations.Add(threshold);

            var outputConnector = acquire.Outputs.First();
            var inputConnector = threshold.Inputs.First();
            outputConnector.IsConnected = true;
            inputConnector.IsConnected = true;
            outputConnector.ValueObservers.Add(inputConnector);
            process.Connections.Add(new ConnectionModel { Input = inputConnector, Output = outputConnector });

            process.SelectedOperation = threshold;

            // Act
            var dto = ProcessMapper.ToDto(process);
            var restored = ProcessMapper.ToModel(dto);

            // Assert
            Assert.Equal(process.Operations.Count, restored.Operations.Count);
            Assert.Equal(process.Connections.Count, restored.Connections.Count);
            Assert.Equal(threshold.Id, restored.SelectedOperation?.Id);

            var restoredConnection = restored.Connections.First();
            Assert.NotNull(restoredConnection.Input);
            Assert.NotNull(restoredConnection.Output);
            Assert.True(restoredConnection.Input!.IsConnected);
            Assert.True(restoredConnection.Output!.IsConnected);
        }

        [Fact]
        public void ProcessSerializer_RoundTrip_PreservesVersionedContainer()
        {
            // Arrange
            var container = new VersionedContainer
            {
                MainView = new MainViewDto
                {
                    AutoSelectNewEditor = true,
                    SelectedEditorId = null,
                    Editors =
                    [
                        new EditorDto
                        {
                            Id = System.Guid.NewGuid(),
                            Name = "Test",
                            Process = new ProcessDto
                            {
                                Operations =
                                [
                                    new OperationDto
                                    {
                                        Id = System.Guid.NewGuid(),
                                        Location = new PointDto { X = 1, Y = 2 },
                                        Size = new SizeDto { Width = 3, Height = 4 }
                                    }
                                ]
                            }
                        }
                    ]
                }
            };

            var serializer = new ProcessSerializer(new MigrationRunner(System.Array.Empty<IMigration>()));

            // Act
            var bytes = serializer.Serialize(container);
            var restored = serializer.Deserialize(bytes);

            // Assert
            Assert.Equal(VersionedContainer.CurrentVersion, restored.Version);
            Assert.NotNull(restored.MainView);
            Assert.Single(restored.MainView.Editors);
            Assert.Equal("Test", restored.MainView.Editors[0].Name);
        }

        [Fact]
        public void MainViewModel_RoundTrip_PreservesEditors()
        {
            // Arrange
            var mainViewModel = new MainViewModel((IDialogService?)null);
            mainViewModel.AddEditorCommand.Execute(null);
            mainViewModel.Editors[0].Name = "Editor A";

            // Act
            var dto = ToDto(mainViewModel);
            var restored = ToModel(dto, null);

            // Assert
            Assert.Equal(mainViewModel.Editors.Count, restored.Editors.Count);
            Assert.Equal("Editor A", restored.Editors[0].Name);
        }

        private static MainViewDto ToDto(MainViewModel model)
        {
            return new MainViewDto
            {
                AutoSelectNewEditor = model.AutoSelectNewEditor,
                SelectedEditorId = model.SelectedEditor?.Id,
                Editors = model.Editors.Select(editor => new EditorDto
                {
                    Id = editor.Id,
                    Name = editor.Name,
                    Process = ProcessMapper.ToDto(editor.Process)
                }).ToList()
            };
        }

        private static MainViewModel ToModel(MainViewDto dto, IDialogService? dialogService)
        {
            var model = new MainViewModel(dialogService);
            model.AutoSelectNewEditor = dto.AutoSelectNewEditor;
            model.Editors.Clear();

            foreach (var editorDto in dto.Editors)
            {
                var process = editorDto.Process is null ? new ProcessModel() : ProcessMapper.ToModel(editorDto.Process);
                var editor = new EditorViewModel(process, editorDto.Id, editorDto.Name);
                editor.Process.OperationsMenu = editor.OperationsMenu;
                model.Editors.Add(editor);
            }

            if (dto.SelectedEditorId.HasValue)
            {
                model.SelectedEditor = model.Editors.FirstOrDefault(e => e.Id == dto.SelectedEditorId.Value);
            }

            return model;
        }
    }
}
