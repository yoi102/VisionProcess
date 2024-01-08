using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace VisionProcess.Core.ToolBase
{
    public interface IOperation
    {
        string? Name { get; set; }
        bool IsRealTime { get; set; }

        event EventHandler? Executed;

        event EventHandler? Executing;

        //IGraphics Graphics { get; }
        //IInputs Inputs { get; }
        //IOutputs Outputs { get; }
        RunStatus RunStatus { get; }

        ObservableCollection<Record> Records { get; }

        void Execute();

        IAsyncRelayCommand? ExecuteCommand { get; }
    }
}