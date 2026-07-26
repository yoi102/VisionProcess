using VisionProcess.Abstractions.ToolBase;
using VisionProcess.Runtime.Models;
using VisionProcess.Tools.Operators;

namespace VisionProcess.Tests;

public sealed class ExecutionPropagationTests
{
    [Fact]
    public async Task ExecutingUpstream_ExecutesConnectedDownstream()
    {
        var process = new ProcessModel();
        var sourceOperator = new TestSourceOperator();
        var targetOperator = new TestTargetOperator();
        var source = new OperationModel { Operator = sourceOperator };
        var target = new OperationModel { Operator = targetOperator };
        process.Operations.Add(source);
        process.Operations.Add(target);

        process.Connections.Add(new ConnectionModel
        {
            Output = source.Outputs.Single(),
            Input = target.Inputs.Single()
        });

        Assert.Equal(0, targetOperator.ExecutionCount);
        var executionCount = 0;
        var targetExecutedTwice = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        targetOperator.Executed += (_, _) =>
        {
            if (Interlocked.Increment(ref executionCount) == 2)
            {
                targetExecutedTwice.TrySetResult();
            }
        };

        await sourceOperator.ExecuteAsync();
        await sourceOperator.ExecuteAsync();
        await targetExecutedTwice.Task.WaitAsync(TimeSpan.FromSeconds(2));

        Assert.Equal(42, targetOperator.Inputs.Value);
        Assert.Equal(2, targetOperator.ExecutionCount);
    }

    [Fact]
    public async Task UpdatingOneUpstream_AfterAllInputsInitialized_ExecutesDownstreamAgain()
    {
        var process = new ProcessModel();
        var firstSourceOperator = new TestSourceOperator();
        var secondSourceOperator = new TestSourceOperator();
        var targetOperator = new TwoInputTargetOperator();
        var firstSource = new OperationModel { Operator = firstSourceOperator };
        var secondSource = new OperationModel { Operator = secondSourceOperator };
        var target = new OperationModel { Operator = targetOperator };
        process.Operations.Add(firstSource);
        process.Operations.Add(secondSource);
        process.Operations.Add(target);

        process.Connections.Add(new ConnectionModel
        {
            Output = firstSource.Outputs.Single(),
            Input = target.Inputs.Single(input => input.ValuePath == "Inputs.First")
        });
        process.Connections.Add(new ConnectionModel
        {
            Output = secondSource.Outputs.Single(),
            Input = target.Inputs.Single(input => input.ValuePath == "Inputs.Second")
        });

        await firstSourceOperator.ExecuteAsync();
        Assert.Equal(0, targetOperator.ExecutionCount);

        await WaitForExecutionAsync(secondSourceOperator, targetOperator, expectedCount: 1);
        await WaitForExecutionAsync(firstSourceOperator, targetOperator, expectedCount: 2);

        Assert.Equal(2, targetOperator.ExecutionCount);
    }

    [Fact]
    public async Task ChangingAcquireImage_PropagatesAndPublishesANewDownstreamImage()
    {
        string testImages = FindTestImagesDirectory();
        var process = new ProcessModel();
        var sourceOperator = new AcquireImageOperator
        {
            ImagePaths =
            [
                Path.Combine(testImages, "0.png"),
                Path.Combine(testImages, "1.png")
            ]
        };
        var targetOperator = new ColorConvertOperator();
        var source = new OperationModel { Operator = sourceOperator };
        var target = new OperationModel { Operator = targetOperator };
        process.Operations.Add(source);
        process.Operations.Add(target);
        process.Connections.Add(new ConnectionModel
        {
            Output = source.Outputs.Single(),
            Input = target.Inputs.Single(input => input.ValuePath == "Inputs.Image")
        });

        await ExecuteAndWaitAsync(sourceOperator, targetOperator);
        Assert.True(targetOperator.RunStatus.Result, targetOperator.RunStatus.Exception?.ToString() ?? targetOperator.RunStatus.Message);
        var firstOutput = targetOperator.Outputs.Image;
        Assert.Equal(0, sourceOperator.Outputs.FrameIndex);
        Assert.Equal(2, sourceOperator.Outputs.FrameCount);
        Assert.Equal("0.png", sourceOperator.Outputs.FileName);
        Assert.True(sourceOperator.Outputs.Width > 0);
        Assert.True(sourceOperator.Outputs.Height > 0);
        Assert.True(sourceOperator.Outputs.Channels > 0);
        Assert.True(sourceOperator.Outputs.HasImage);

        await ExecuteAndWaitAsync(sourceOperator, targetOperator);
        Assert.True(targetOperator.RunStatus.Result, targetOperator.RunStatus.Exception?.ToString() ?? targetOperator.RunStatus.Message);
        var secondOutput = targetOperator.Outputs.Image;
        Assert.Equal(1, sourceOperator.Outputs.FrameIndex);
        Assert.Equal("1.png", sourceOperator.Outputs.FileName);

        Assert.NotNull(firstOutput);
        Assert.NotNull(secondOutput);
        Assert.NotSame(firstOutput, secondOutput);
        Assert.NotSame(sourceOperator.Outputs.Image, targetOperator.Inputs.Image);
    }

    private static async Task ExecuteAndWaitAsync(
        AcquireImageOperator source,
        ColorConvertOperator target)
    {
        var executed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        EventHandler handler = (_, _) => executed.TrySetResult();
        target.Executed += handler;
        try
        {
            await source.ExecuteAsync();
            Assert.True(source.RunStatus.Result, source.RunStatus.Exception?.ToString() ?? source.RunStatus.Message);
            Assert.NotNull(source.Outputs.Image);
            await executed.Task.WaitAsync(TimeSpan.FromSeconds(5));
        }
        finally
        {
            target.Executed -= handler;
        }
    }

    private static string FindTestImagesDirectory()
    {
        for (var directory = new DirectoryInfo(AppContext.BaseDirectory);
             directory is not null;
             directory = directory.Parent)
        {
            string candidate = Path.Combine(directory.FullName, "TestImages");
            if (Directory.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new DirectoryNotFoundException("TestImages directory was not found.");
    }

    private static async Task WaitForExecutionAsync(
        TestSourceOperator source,
        TwoInputTargetOperator target,
        int expectedCount)
    {
        var executed = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        EventHandler handler = (_, _) =>
        {
            if (target.ExecutionCount >= expectedCount)
            {
                executed.TrySetResult();
            }
        };
        target.Executed += handler;
        try
        {
            await source.ExecuteAsync();
            await executed.Task.WaitAsync(TimeSpan.FromSeconds(2));
        }
        finally
        {
            target.Executed -= handler;
        }
    }

    private sealed class TestInputs : InputsBase
    {
        public int Value { get; set; }

        protected override void ConfigureConnectors(ConnectorProviderBuilder builder)
            => builder.Add<int>(nameof(Value), nameof(Value), () => Value, value => Value = value);
    }

    private sealed class TestOutputs : OutputsBase
    {
        public int Value { get; set; }

        protected override void ConfigureConnectors(ConnectorProviderBuilder builder)
            => builder.Add<int>(nameof(Value), nameof(Value), () => Value, null);
    }

    private sealed class TwoTestInputs : InputsBase
    {
        public int First { get; set; }

        public int Second { get; set; }

        protected override void ConfigureConnectors(ConnectorProviderBuilder builder)
        {
            builder.Add<int>(nameof(First), nameof(First), () => First, value => First = value);
            builder.Add<int>(nameof(Second), nameof(Second), () => Second, value => Second = value);
        }
    }

    private sealed class TestSourceOperator : OperatorBase<InputsEmpty, TestOutputs, GraphicsEmpty>
    {
        protected override bool InternalExecute(out string message)
        {
            Outputs.Value = 42;
            message = string.Empty;
            return true;
        }

        protected override IEnumerable<DefaultConnectorInfo> GetDefaultConnectors()
        {
            yield return new DefaultConnectorInfo(false, "Value", "Outputs.Value");
        }
    }

    private sealed class TestTargetOperator : OperatorBase<TestInputs, OutputsEmpty, GraphicsEmpty>
    {
        public int ExecutionCount { get; private set; }

        protected override bool InternalExecute(out string message)
        {
            ExecutionCount++;
            message = string.Empty;
            return true;
        }

        protected override IEnumerable<DefaultConnectorInfo> GetDefaultConnectors()
        {
            yield return new DefaultConnectorInfo(true, "Value", "Inputs.Value");
        }
    }

    private sealed class TwoInputTargetOperator : OperatorBase<TwoTestInputs, OutputsEmpty, GraphicsEmpty>
    {
        public int ExecutionCount { get; private set; }

        protected override bool InternalExecute(out string message)
        {
            ExecutionCount++;
            message = string.Empty;
            return true;
        }

        protected override IEnumerable<DefaultConnectorInfo> GetDefaultConnectors()
        {
            yield return new DefaultConnectorInfo(true, "First", "Inputs.First");
            yield return new DefaultConnectorInfo(true, "Second", "Inputs.Second");
        }
    }
}
