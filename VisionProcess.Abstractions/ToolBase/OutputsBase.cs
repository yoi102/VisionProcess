using CommunityToolkit.Mvvm.ComponentModel;

namespace VisionProcess.Abstractions.ToolBase
{
    /// <summary>
    /// 输出对象基类，提供显式连接器注册能力。
    /// </summary>
    public abstract class OutputsBase : ObservableObject, IOutputs
    {
        private IConnectorProvider? _provider;

        private IConnectorProvider Provider => _provider ??= BuildProvider();

        private IConnectorProvider BuildProvider()
        {
            var builder = new ConnectorProviderBuilder();
            ConfigureConnectors(builder);
            return new ConnectorProvider(builder.Build());
        }

        /// <summary>
        /// 派生类在此注册可被节点编辑器连接的属性。
        /// </summary>
        protected abstract void ConfigureConnectors(ConnectorProviderBuilder builder);

        public IEnumerable<IConnectorDescriptor> GetConnectors() => Provider.GetConnectors();

        public object? GetValue(string path) => Provider.GetValue(path);

        public void SetValue(string path, object? value) => Provider.SetValue(path, value);

        public Type? GetValueType(string path) => Provider.GetValueType(path);
    }

    /// <summary>
    /// 无输出的算子使用的空输出对象。
    /// </summary>
    public class OutputsEmpty : OutputsBase
    {
        protected override void ConfigureConnectors(ConnectorProviderBuilder builder)
        {
            // 无连接器
        }
    }
}
