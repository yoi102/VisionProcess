using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace VisionProcess.Core.ToolBase
{
    public interface IOperator
    {
        event EventHandler? Executed;

        event EventHandler? Executing;

        IAsyncRelayCommand? ExecuteCommand { get; }
        bool IsRealTime { get; set; }
        string? Name { get; set; }
        ObservableCollection<Record> Records { get; }

        IGraphics Graphics { get; }
        IInputs Inputs { get; }
        IOutputs Outputs { get; }
        RunStatus RunStatus { get; }

        void Execute();
    }
}