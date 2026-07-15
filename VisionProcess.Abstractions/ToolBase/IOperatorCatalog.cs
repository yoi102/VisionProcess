namespace VisionProcess.Abstractions.ToolBase;

/// <summary>
/// Read-only catalog of operator modules registered in the platform.
/// </summary>
public interface IOperatorCatalog
{
    IReadOnlyList<OperatorDescriptor> Operators { get; }

    OperatorDescriptor Get(string id);

    bool TryGet(string id, out OperatorDescriptor descriptor);

    bool TryGet(Type operatorType, out OperatorDescriptor descriptor);
}
