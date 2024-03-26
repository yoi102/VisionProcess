using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace VisionProcess.Core.ToolBase
{
    public interface IOperator
    {
        event EventHandler? Executed;

        event EventHandler? Executing;

        IAsyncRelayCommand? ExecuteCommand { get; }
        IGraphics Graphics { get; }
        IInputs Inputs { get; }
        bool IsRealTime { get; set; }
        string? Name { get; set; }
        IOutputs Outputs { get; }
        ObservableCollection<Record> Records { get; }
        RunStatus RunStatus { get; }

        Task ExecuteAsync();
    }
}