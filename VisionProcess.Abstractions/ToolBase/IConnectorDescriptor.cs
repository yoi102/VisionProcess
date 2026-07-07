namespace VisionProcess.Abstractions.ToolBase
{
    /// <summary>
    /// 描述一个可被节点编辑器连接的成员。
    /// </summary>
    public interface IConnectorDescriptor
    {
        /// <summary>
        /// 连接点路径，如 "Image"、"KernelSize"。
        /// </summary>
        string Path { get; }

        /// <summary>
        /// 连接点显示标题。
        /// </summary>
        string Title { get; }

        /// <summary>
        /// 连接点值的类型。
        /// </summary>
        Type ValueType { get; }

        /// <summary>
        /// 是否可读（作为输出）。
        /// </summary>
        bool CanRead { get; }

        /// <summary>
        /// 是否可写（作为输入）。
        /// </summary>
        bool CanWrite { get; }

        /// <summary>
        /// 获取当前值。
        /// </summary>
        object? GetValue();

        /// <summary>
        /// 设置当前值。
        /// </summary>
        void SetValue(object? value);
    }
}
