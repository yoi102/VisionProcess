using System;
using System.Linq;
using OpenCvSharp;
using VisionProcess.Abstractions.ToolBase;
using VisionProcess.Common.Primitives;
using VisionProcess.Persistence;
using VisionProcess.Persistence.Migrations;
using VisionProcess.Persistence.Operators;
using VisionProcess.Runtime.Models;
using VisionProcess.Tools.Models;
using VisionProcess.Tools.Operators;
using VisionProcess.Tools.Services;
using VisionProcess.ViewModel.Tooling;
using VisionProcess.ViewModel.ViewModels;
using VisionProcess.ViewModel.ViewModels.Tools;
using VisionProcess.ViewModel.ViewModels.Toolboxes;
using VisionProcess.ViewModel.ViewServices;
using Xunit;

namespace VisionProcess.Tests
{
    public class PersistenceRoundTripTests
    {
        public PersistenceRoundTripTests()
        {
            TestIoc.EnsureConfigured();
        }

        [Fact]
        public void ProcessMapper_RoundTrip_PreservesOperationsAndConnections()
        {
            // Arrange
            var process = new ProcessModel();

            var factory = new OperatorFactory();
            var acquire = new OperationModel { Operator = factory.Create("AcquireImage"), Location = new VpPoint(10, 20) };
            var threshold = new OperationModel { Operator = factory.Create("ImageThreshold"), Location = new VpPoint(100, 200) };

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

            var restoredAcquire = restored.Operations[0];
            var restoredThreshold = restored.Operations[1];
            Assert.Same(restoredAcquire.Outputs[0], restoredConnection.Output);
            Assert.Same(restoredThreshold.Inputs[0], restoredConnection.Input);
            Assert.Single(restoredConnection.Output.ValueObservers);
            Assert.Same(restoredConnection.Input, restoredConnection.Output.ValueObservers[0]);
        }

        [Fact]
        public void ProcessMapper_RoundTrip_PreservesOperatorState()
        {
            // Arrange
            var process = new ProcessModel();
            var filter = new OperationModel { Operator = new ImageFilterOperator(), Location = new VpPoint(10, 20) };
            var filterOp = (ImageFilterOperator)filter.Operator!;
            filterOp.Inputs.FilterType = FilterTypes.Gaussian;
            filterOp.Inputs.KernelWidth = 5;
            filterOp.Inputs.KernelHeight = 7;
            filterOp.Inputs.SigmaX = 1.5;
            filterOp.Name = "Filter";
            filterOp.RunStatus.Message = "Done";
            filterOp.RunStatus.Result = true;

            var acquire = new OperationModel { Operator = new AcquireImageOperator(), Location = new VpPoint(100, 200) };
            var acquireOp = (AcquireImageOperator)acquire.Operator!;
            acquireOp.CurrentIndex = 2;
            acquireOp.ImagePaths = new List<string> { "a.png", "b.png" };

            process.Operations.Add(filter);
            process.Operations.Add(acquire);

            // Act
            var dto = ProcessMapper.ToDto(process);
            var restored = ProcessMapper.ToModel(dto);

            // Assert
            var restoredFilter = restored.Operations[0].Operator as ImageFilterOperator;
            Assert.NotNull(restoredFilter);
            Assert.Equal(FilterTypes.Gaussian, restoredFilter.Inputs.FilterType);
            Assert.Equal(5, restoredFilter.Inputs.KernelWidth);
            Assert.Equal(7, restoredFilter.Inputs.KernelHeight);
            Assert.Equal(1.5, restoredFilter.Inputs.SigmaX);
            Assert.Equal("Filter", restoredFilter.Name);
            Assert.Equal("Done", restoredFilter.RunStatus.Message);
            Assert.True(restoredFilter.RunStatus.Result);

            var restoredAcquire = restored.Operations[1].Operator as AcquireImageOperator;
            Assert.NotNull(restoredAcquire);
            Assert.Equal(2, restoredAcquire.CurrentIndex);
            Assert.Equal(new[] { "a.png", "b.png" }, restoredAcquire.ImagePaths);
        }

        [Fact]
        public void ProcessMapper_RoundTrip_PreservesColorConvertType()
        {
            // Arrange
            var process = new ProcessModel();
            var color = new OperationModel { Operator = new ColorConvertOperator(), Location = new VpPoint(10, 20) };
            var colorOp = (ColorConvertOperator)color.Operator!;
            colorOp.Inputs.ColorConversionCodes = ColorConvertType.RGB2GRAY;

            process.Operations.Add(color);

            // Act
            var dto = ProcessMapper.ToDto(process);
            var restored = ProcessMapper.ToModel(dto);

            // Assert
            var restoredColor = restored.Operations[0].Operator as ColorConvertOperator;
            Assert.NotNull(restoredColor);
            Assert.Equal(ColorConvertType.RGB2GRAY, restoredColor.Inputs.ColorConversionCodes);
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
                            Id = Guid.NewGuid(),
                            Name = "Test",
                            Process = new ProcessDto
                            {
                                Operations =
                                [
                                    new OperationDto
                                    {
                                        Id = Guid.NewGuid(),
                                        Location = new PointDto { X = 1, Y = 2 },
                                        Size = new SizeDto { Width = 3, Height = 4 },
                                        Operator = new ImageThresholdOperatorDto
                                        {
                                            Name = "Threshold",
                                            ThresholdValue = 128,
                                            MaximumValue = 255,
                                            ThresholdType = ThresholdTypes.Binary
                                        }
                                    }
                                ]
                            }
                        }
                    ]
                }
            };

            var serializer = new ProcessSerializer(new MigrationRunner(Array.Empty<IMigration>()));

            // Act
            var bytes = serializer.Serialize(container);
            var restored = serializer.Deserialize(bytes);

            // Assert
            Assert.Equal(VersionedContainer.CurrentVersion, restored.Version);
            Assert.NotNull(restored.MainView);
            Assert.Single(restored.MainView.Editors);
            Assert.Equal("Test", restored.MainView.Editors[0].Name);
            var restoredOperator = Assert.IsType<ImageThresholdOperatorDto>(restored.MainView!.Editors[0].Process!.Operations[0].Operator);
            Assert.Equal("Threshold", restoredOperator.Name);
            Assert.Equal(128, restoredOperator.ThresholdValue);
            Assert.Equal(255, restoredOperator.MaximumValue);
            Assert.Equal(ThresholdTypes.Binary, restoredOperator.ThresholdType);
        }

        [Fact]
        public void MainViewModel_RoundTrip_PreservesEditors()
        {
            // Arrange
            var mainViewModel = CreateMainViewModel();
            mainViewModel.AddEditorCommand.Execute(null);
            mainViewModel.Editors[0].Name = "Editor A";

            // Act
            var dto = ToDto(mainViewModel);
            var restored = ToModel(dto);

            // Assert
            Assert.Equal(mainViewModel.Editors.Count, restored.Editors.Count);
            Assert.Equal("Editor A", restored.Editors[0].Name);
        }

        [Fact]
        public void EditorViewModel_DtoConstructor_AttachesOperatorViewModels()
        {
            // Arrange
            TestIoc.EnsureConfigured();
            var process = new ProcessModel();
            process.Operations.Add(new OperationModel { Operator = new AcquireImageOperator() });

            // Act
            var editor = CreateEditorFactory().Create(process, Guid.NewGuid(), "Test");

            // Assert
            Assert.IsType<AcquireImageViewModel>(editor.Process.Operations[0].OperatorViewModel);
        }

        [Fact]
        public void OperationsMenuViewModel_CreateOperation_SetsOperatorViewModel()
        {
            // Arrange
            TestIoc.EnsureConfigured();
            var process = new ProcessModel();
            var menu = new OperationsMenuViewModel(
                process,
                new OperatorFactory(),
                new OperatorViewModelFactory(new StubFileDialogService()));
            menu.OpenAt(new VpPoint(0, 0));

            // Act
            menu.CreateOperationCommand.Execute(ToolRegistry.GetToolName(typeof(AcquireImageOperator)));

            // Assert
            Assert.Single(process.Operations);
            Assert.IsType<AcquireImageViewModel>(process.Operations[0].OperatorViewModel);
        }

        [Fact]
        public void EditorViewModel_OpenIOConnector_UsesViewService()
        {
            var dialogService = new StubWindowDialogService();
            var editor = CreateEditorFactory(dialogService).Create();
            var operation = new OperationModel { Operator = new AcquireImageOperator() };

            editor.OpenIOConnectorCommand.Execute(operation);

            Assert.Same(operation, dialogService.OpenedOperation);
        }

        private static MainViewModel CreateMainViewModel(
            IWindowDialogService? windowDialogService = null,
            IOperatorFactory? operatorFactory = null,
            IOperatorViewModelFactory? viewModelFactory = null)
        {
            var folderExplorer = new FolderExplorerToolboxViewModel(new StubFileDocumentService());
            var properties = new PropertiesToolboxViewModel(new StubToolboxIconsService());
            var editorFactory = CreateEditorFactory(windowDialogService, operatorFactory, viewModelFactory);

            return new MainViewModel(
                new StubCultureSettingService(),
                new StubThemeSettingService(),
                editorFactory,
                folderExplorer,
                properties);
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

        private static MainViewModel ToModel(MainViewDto dto)
        {
            var model = CreateMainViewModel();
            model.AutoSelectNewEditor = dto.AutoSelectNewEditor;
            model.Editors.Clear();

            foreach (var editorDto in dto.Editors)
            {
                var process = editorDto.Process is null ? new ProcessModel() : ProcessMapper.ToModel(editorDto.Process);
                var editor = CreateEditorFactory().Create(process, editorDto.Id, editorDto.Name);
                model.Editors.Add(editor);
            }

            if (dto.SelectedEditorId.HasValue)
            {
                model.SelectedEditor = model.Editors.FirstOrDefault(e => e.Id == dto.SelectedEditorId.Value);
            }

            return model;
        }

        private static IEditorViewModelFactory CreateEditorFactory(
            IWindowDialogService? windowDialogService = null,
            IOperatorFactory? operatorFactory = null,
            IOperatorViewModelFactory? viewModelFactory = null)
            => new EditorViewModelFactory(
                windowDialogService ?? new StubWindowDialogService(),
                operatorFactory ?? new OperatorFactory(),
                viewModelFactory ?? new OperatorViewModelFactory(new StubFileDialogService()));
    }
}
