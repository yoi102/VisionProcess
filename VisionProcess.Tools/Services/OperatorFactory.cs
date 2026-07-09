using System;
using System.Collections.Generic;
using VisionProcess.Abstractions.ToolBase;
using VisionProcess.Tools.Operators;

namespace VisionProcess.Tools.Services
{
    /// <summary>
    /// 算子工厂。按名称创建算子实例（IOperator）。
    /// </summary>
    public sealed class OperatorFactory : IOperatorFactory
    {
        private readonly Dictionary<string, Func<IOperator>> _factories;

        public OperatorFactory()
        {
            _factories = new Dictionary<string, Func<IOperator>>(StringComparer.Ordinal)
            {
                [ToolRegistry.GetToolName(typeof(AcquireImageOperator))] = () => new AcquireImageOperator(),
                [ToolRegistry.GetToolName(typeof(ImageFilterOperator))] = () => new ImageFilterOperator(),
                [ToolRegistry.GetToolName(typeof(ImageThresholdOperator))] = () => new ImageThresholdOperator(),
                [ToolRegistry.GetToolName(typeof(ColorConvertOperator))] = () => new ColorConvertOperator(),
            };
        }

        /// <summary>
        /// 根据算子名称创建实例。
        /// </summary>
        public IOperator Create(string name)
        {
            if (!_factories.TryGetValue(name, out var factory))
                throw new ArgumentException($"Unknown operator '{name}'.", nameof(name));
            return factory();
        }

        /// <summary>
        /// 判断是否存在指定名称的算子。
        /// </summary>
        public bool Exists(string name) => _factories.ContainsKey(name);
    }
}
