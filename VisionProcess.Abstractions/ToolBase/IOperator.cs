using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace VisionProcess.Abstractions.ToolBase
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

        /// <summary>
        /// 获取算子默认提供的输入/输出连接器定义。
        /// </summary>
        IEnumerable<DefaultConnectorInfo> GetDefaultConnectors();
    }
}
