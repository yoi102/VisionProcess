using System;
using VisionProcess.Abstractions.ToolBase;

namespace VisionProcess.Tools.Services
{
    /// <summary>
    /// 算子工厂。按名称创建算子实例（IOperator）。
    /// </summary>
    public sealed class OperatorFactory : IOperatorFactory
    {
        private readonly IOperatorCatalog catalog;

        public OperatorFactory()
            : this(new OperatorCatalog(BuiltInOperatorDescriptors.All))
        {
        }

        public OperatorFactory(IOperatorCatalog catalog)
            => this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));

        /// <summary>
        /// 根据算子名称创建实例。
        /// </summary>
        public IOperator Create(string name) => catalog.Get(name).CreateOperator();

        /// <summary>
        /// 判断是否存在指定名称的算子。
        /// </summary>
        public bool Exists(string name) => catalog.TryGet(name, out _);
    }
}
