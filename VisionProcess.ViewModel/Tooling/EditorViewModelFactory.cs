using VisionProcess.Abstractions.ToolBase;
using VisionProcess.Runtime.Models;
using VisionProcess.ViewModel.ViewModels;
using VisionProcess.ViewModel.ViewServices;

namespace VisionProcess.ViewModel.Tooling;

public sealed class EditorViewModelFactory : IEditorViewModelFactory
{
    private readonly IWindowDialogService _windowDialogService;
    private readonly IOperatorFactory _operatorFactory;
    private readonly IOperatorViewModelFactory _operatorViewModelFactory;

    public EditorViewModelFactory(
        IWindowDialogService windowDialogService,
        IOperatorFactory operatorFactory,
        IOperatorViewModelFactory operatorViewModelFactory)
    {
        _windowDialogService = windowDialogService;
        _operatorFactory = operatorFactory;
        _operatorViewModelFactory = operatorViewModelFactory;
    }

    public EditorViewModel Create()
        => new(
            new ProcessModel(),
            Guid.NewGuid(),
            null,
            _windowDialogService,
            _operatorFactory,
            _operatorViewModelFactory);

    public EditorViewModel Create(ProcessModel process, Guid id, string? name)
        => new(
            process,
            id,
            name,
            _windowDialogService,
            _operatorFactory,
            _operatorViewModelFactory);
}
