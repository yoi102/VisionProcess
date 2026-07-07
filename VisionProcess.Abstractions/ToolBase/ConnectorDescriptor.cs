namespace VisionProcess.Abstractions.ToolBase
{
    /// <summary>
    /// 连接器描述实现。内部使用 object 委托以避免泛型可空约束问题。
    /// </summary>
    public sealed class ConnectorDescriptor : IConnectorDescriptor
    {
        private readonly Func<object?>? _getter;
        private readonly Action<object?>? _setter;

        public ConnectorDescriptor(string path, string title, Type valueType, Func<object?>? getter, Action<object?>? setter)
        {
            Path = path;
            Title = title;
            ValueType = valueType;
            _getter = getter;
            _setter = setter;
        }

        public string Path { get; }

        public string Title { get; }

        public Type ValueType { get; }

        public bool CanRead => _getter is not null;

        public bool CanWrite => _setter is not null;

        public object? GetValue() => _getter?.Invoke();

        public void SetValue(object? value) => _setter?.Invoke(value);
    }
}
