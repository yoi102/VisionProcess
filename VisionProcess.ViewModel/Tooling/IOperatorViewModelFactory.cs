using VisionProcess.Abstractions.ToolBase;

namespace VisionProcess.ViewModel.Tooling
{
    /// <summary>
    /// 根据算子实例创建对应 ViewModel 的工厂抽象。
    /// </summary>
    public interface IOperatorViewModelFactory
    {
        /// <summary>
        /// 为指定算子创建对应的 ViewModel。
        /// </summary>
        object Create(IOperator @operator);
    }
}
