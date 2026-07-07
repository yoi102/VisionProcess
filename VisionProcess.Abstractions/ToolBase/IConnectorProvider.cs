namespace VisionProcess.Abstractions.ToolBase
{
    /// <summary>
    /// 为算子或输入/输出对象提供显式的连接器描述与访问能力。
    /// 实现该接口后，流程引擎不再需要通过反射读取属性。
    /// </summary>
    public interface IConnectorProvider
    {
        /// <summary>
        /// 获取所有可用的连接器描述。
        /// </summary>
        IEnumerable<IConnectorDescriptor> GetConnectors();

        /// <summary>
        /// 按路径获取值。
        /// </summary>
        object? GetValue(string path);

        /// <summary>
        /// 按路径设置值。
        /// </summary>
        void SetValue(string path, object? value);

        /// <summary>
        /// 按路径获取值类型。
        /// </summary>
        Type? GetValueType(string path);
    }
}
