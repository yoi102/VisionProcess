using System.ComponentModel;

namespace VisionProcess.Abstractions.ToolBase
{
    /// <summary>
    /// 算子输出对象接口，同时提供属性变更通知与显式连接器访问能力。
    /// </summary>
    public interface IOutputs : INotifyPropertyChanged, IConnectorProvider
    {
    }
}
