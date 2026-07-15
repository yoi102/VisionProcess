using VisionProcess.Abstractions.Services;
using VisionProcess.Abstractions.ToolBase;
using VisionProcess.Tools.Services;

namespace VisionProcess.ViewModel.Tooling;

/// <summary>
/// The operators shipped with the desktop application, exposed through the same contract as plugins.
/// </summary>
public sealed class BuiltInOperatorModule : IOperatorModule
{
    public BuiltInOperatorModule(IFileDialogService? fileDialogService = null)
    {
        Operators = BuiltInOperatorDescriptors.All;
        ViewModels = BuiltInOperatorViewModels.Create(fileDialogService);
    }

    public IReadOnlyList<OperatorDescriptor> Operators { get; }

    public IReadOnlyList<IOperatorViewModelRegistration> ViewModels { get; }
}
