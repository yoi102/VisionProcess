using VisionProcess.Runtime.Models;
using VisionProcess.ViewModel.ViewModels;

namespace VisionProcess.ViewModel.Tooling;

public interface IEditorViewModelFactory
{
    EditorViewModel Create();

    EditorViewModel Create(ProcessModel process, Guid id, string? name);
}
