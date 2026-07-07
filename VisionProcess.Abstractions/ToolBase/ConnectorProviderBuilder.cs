namespace VisionProcess.Abstractions.ToolBase
{
    /// <summary>
    /// 用于构造 <see cref="IConnectorProvider"/> 的辅助类。
    /// </summary>
    public sealed class ConnectorProviderBuilder
    {
        private readonly List<IConnectorDescriptor> _descriptors = new();

        /// <summary>
        /// 注册一个可连接属性。
        /// </summary>
        public ConnectorProviderBuilder Add<T>(string path, string title, Func<T>? getter, Action<T>? setter = null)
        {
            Func<object?>? objectGetter = getter is null ? null : () => getter();
            Action<object?>? objectSetter = setter is null
                ? null
                : v =>
                {
                    if (v is null)
                        setter(default!);
                    else
                        setter((T)v);
                };

            _descriptors.Add(new ConnectorDescriptor(path, title, typeof(T), objectGetter, objectSetter));
            return this;
        }

        /// <summary>
        /// 构建只读描述符列表。
        /// </summary>
        public IReadOnlyList<IConnectorDescriptor> Build() => _descriptors.AsReadOnly();
    }
}
