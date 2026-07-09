namespace VisionProcess.Abstractions.ToolBase
{
    /// <summary>
    /// 算子工厂抽象。由具体实现负责在创建算子时注入其所需服务。
    /// </summary>
    public interface IOperatorFactory
    {
        /// <summary>
        /// 根据算子名称创建实例。
        /// </summary>
        IOperator Create(string name);

        /// <summary>
        /// 判断是否存在指定名称的算子。
        /// </summary>
        bool Exists(string name);
    }
}
