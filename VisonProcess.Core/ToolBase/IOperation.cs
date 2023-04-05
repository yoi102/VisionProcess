using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace VisonProcess.Core.ToolBase
{
    public interface IOperation
    {
        event EventHandler? Executed;
        event EventHandler? Executing;

        //IGraphics Graphic { get; }
        //IInputs Inputs { get; }
        //IOutputs Outputs { get; }
        RunStatus RunStatus { get; }

        ObservableCollection<Record> Records { get; }

        void Execute();

        IAsyncRelayCommand? ExecuteCommand { get; }
    }
}