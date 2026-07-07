namespace VisionProcess.Abstractions.ToolBase
{
    /// <summary>
    /// <see cref="IConnectorProvider"/> 的默认实现。
    /// </summary>
    public sealed class ConnectorProvider : IConnectorProvider
    {
        private readonly IReadOnlyDictionary<string, IConnectorDescriptor> _descriptorsByPath;

        public ConnectorProvider(IEnumerable<IConnectorDescriptor> descriptors)
        {
            _descriptorsByPath = descriptors.ToDictionary(d => d.Path);
        }

        public IEnumerable<IConnectorDescriptor> GetConnectors() => _descriptorsByPath.Values;

        public object? GetValue(string path)
        {
            if (!_descriptorsByPath.TryGetValue(path, out var descriptor))
                return null;
            return descriptor.GetValue();
        }

        public void SetValue(string path, object? value)
        {
            if (!_descriptorsByPath.TryGetValue(path, out var descriptor))
                return;
            descriptor.SetValue(value);
        }

        public Type? GetValueType(string path)
        {
            if (!_descriptorsByPath.TryGetValue(path, out var descriptor))
                return null;
            return descriptor.ValueType;
        }
    }
}
