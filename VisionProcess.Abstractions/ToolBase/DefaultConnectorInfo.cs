namespace VisionProcess.Abstractions.ToolBase
{
    /// <summary>
    /// 描述算子默认提供的输入/输出连接器。
    /// 替代运行时的 <see cref="DefaultToolConnectorAttribute"/> 反射读取。
    /// </summary>
    public class DefaultConnectorInfo
    {
        public DefaultConnectorInfo(bool isInput, string title, string path)
        {
            IsInput = isInput;
            Title = title;
            Path = path;
        }

        /// <summary>
        /// 是否为输入连接器。
        /// </summary>
        public bool IsInput { get; }

        /// <summary>
        /// 显示标题。
        /// </summary>
        public string Title { get; }

        /// <summary>
        /// 连接器路径，如 "Outputs.Image"。
        /// </summary>
        public string Path { get; }
    }
}
