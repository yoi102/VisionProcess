using VisionProcess.Abstractions.ToolBase;
using VisionProcess.Persistence;
using VisionProcess.Runtime.Models;
using VisionProcess.Tools.Operators;
using VisionProcess.Tools.Services;
using VisionProcess.ViewModel.ViewModels;

namespace VisionProcess.Tests;

public sealed class IOConnectorViewModelTests
{
    [Fact]
    public void AcquireImage_OffersImageMetadataAndSourceOutputPorts()
    {
        var operation = new OperationModel { Operator = new AcquireImageOperator() };
        var viewModel = new IOConnectorViewModel(operation);

        var outputPaths = viewModel.TreeNodes
            .Single(node => node.Path == "Outputs")
            .ChildNodes
            .Select(node => node.FullPath)
            .ToArray();

        Assert.Equal(11, viewModel.OutputPortItemCount);
        Assert.Contains("Outputs.Image", outputPaths);
        Assert.Contains("Outputs.Width", outputPaths);
        Assert.Contains("Outputs.Height", outputPaths);
        Assert.Contains("Outputs.Channels", outputPaths);
        Assert.Contains("Outputs.PixelFormat", outputPaths);
        Assert.Contains("Outputs.PixelCount", outputPaths);
        Assert.Contains("Outputs.HasImage", outputPaths);
        Assert.Contains("Outputs.FilePath", outputPaths);
        Assert.Contains("Outputs.FileName", outputPaths);
        Assert.Contains("Outputs.FrameIndex", outputPaths);
        Assert.Contains("Outputs.FrameCount", outputPaths);
    }

    [Fact]
    public void AddAllPorts_AddsEveryAvailablePortWithoutDuplicatingDefaults()
    {
        var operation = new OperationModel { Operator = new ImageFilterOperator() };
        var viewModel = new IOConnectorViewModel(operation);

        viewModel.AddAllInputsCommand.Execute(null);
        viewModel.AddAllOutputsCommand.Execute(null);
        viewModel.AddAllInputsCommand.Execute(null);
        viewModel.AddAllOutputsCommand.Execute(null);

        Assert.Equal(11, viewModel.InputPortItemCount);
        Assert.Equal(7, viewModel.OutputPortItemCount);
        Assert.Equal(11, operation.Inputs.Count);
        Assert.Equal(7, operation.Outputs.Count);
        Assert.Equal(
            operation.Inputs.Count,
            operation.Inputs.Select(connector => connector.ValuePath).Distinct().Count());
        Assert.Equal(
            operation.Outputs.Count,
            operation.Outputs.Select(connector => connector.ValuePath).Distinct().Count());
    }

    [Fact]
    public void AddedMetadataPorts_RoundTripThroughPersistence()
    {
        var operation = new OperationModel { Operator = new AcquireImageOperator() };
        var viewModel = new IOConnectorViewModel(operation);
        viewModel.AddAllOutputsCommand.Execute(null);
        var process = new ProcessModel();
        process.Operations.Add(operation);
        var mapper = new ProcessMapper(
            new OperatorCatalog(BuiltInOperatorDescriptors.All));

        var restored = mapper.ToModel(mapper.ToDto(process));

        var restoredOperation = Assert.Single(restored.Operations);
        Assert.Equal(11, restoredOperation.Outputs.Count);
        Assert.Equal(
            typeof(int),
            restoredOperation.Outputs.Single(
                connector => connector.ValuePath == "Outputs.Width").ValueType);
        Assert.Equal(
            typeof(string),
            restoredOperation.Outputs.Single(
                connector => connector.ValuePath == "Outputs.FilePath").ValueType);
    }
}
