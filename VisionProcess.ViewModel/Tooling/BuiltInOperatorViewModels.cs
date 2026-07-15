using VisionProcess.Abstractions.Services;
using VisionProcess.Tools.Operators;
using VisionProcess.ViewModel.ViewModels.Tools;

namespace VisionProcess.ViewModel.Tooling;

public static class BuiltInOperatorViewModels
{
    public static IReadOnlyList<IOperatorViewModelRegistration> Create(
        IFileDialogService? fileDialogService = null)
        =>
        [
            new OperatorViewModelRegistration<AcquireImageOperator>(
                op => new AcquireImageViewModel(op, fileDialogService)),
            new OperatorViewModelRegistration<ColorConvertOperator>(
                static op => new ColorConvertViewModel(op)),
            new OperatorViewModelRegistration<ImageFilterOperator>(
                static op => new ImageFilterViewModel(op)),
            new OperatorViewModelRegistration<ImageThresholdOperator>(
                static op => new ImageThresholdViewModel(op))
        ];
}
