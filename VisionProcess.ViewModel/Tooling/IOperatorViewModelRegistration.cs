using VisionProcess.Abstractions.ToolBase;

namespace VisionProcess.ViewModel.Tooling;

/// <summary>
/// Creates the editor-facing ViewModel for one operator type.
/// Module UI packages can add registrations without changing the core factory.
/// </summary>
public interface IOperatorViewModelRegistration
{
    Type OperatorType { get; }

    object Create(IOperator @operator);
}

public sealed class OperatorViewModelRegistration<TOperator> : IOperatorViewModelRegistration
    where TOperator : class, IOperator
{
    private readonly Func<TOperator, object> factory;

    public OperatorViewModelRegistration(Func<TOperator, object> factory)
        => this.factory = factory ?? throw new ArgumentNullException(nameof(factory));

    public Type OperatorType => typeof(TOperator);

    public object Create(IOperator @operator)
        => factory(@operator as TOperator
            ?? throw new ArgumentException(
                $"Expected operator '{typeof(TOperator)}', received '{@operator.GetType()}'.",
                nameof(@operator)));
}
