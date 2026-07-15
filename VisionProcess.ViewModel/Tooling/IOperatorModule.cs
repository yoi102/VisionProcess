using VisionProcess.Abstractions.ToolBase;

namespace VisionProcess.ViewModel.Tooling;

/// <summary>
/// One composable vision module package: runtime operators plus their editor UI registrations.
/// </summary>
public interface IOperatorModule
{
    IReadOnlyList<OperatorDescriptor> Operators { get; }

    IReadOnlyList<IOperatorViewModelRegistration> ViewModels { get; }
}
