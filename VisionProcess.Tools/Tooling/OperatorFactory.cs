using System;
using System.Collections.Generic;
using VisionProcess.Abstractions.ToolBase;
using VisionProcess.Tools.ViewModels;

namespace VisionProcess.Tools.Tooling
{
    /// <summary>
    /// 算子工厂。按名称创建算子实例，替代反射式的 <see cref="Activator.CreateInstance(Type)"/>。
    /// </summary>
    public static class OperatorFactory
    {
        private static readonly Dictionary<string, Func<IOperator>> Factories = new(StringComparer.Ordinal)
        {
            [ToolRegistry.GetToolName(typeof(AcquireImageViewModel))] = () => new AcquireImageViewModel(),
            [ToolRegistry.GetToolName(typeof(ImageFilterViewModel))] = () => new ImageFilterViewModel(),
            [ToolRegistry.GetToolName(typeof(ImageThresholdViewModel))] = () => new ImageThresholdViewModel(),
            [ToolRegistry.GetToolName(typeof(ColorConvertViewModel))] = () => new ColorConvertViewModel(),
        };

        /// <summary>
        /// 根据算子名称创建实例。
        /// </summary>
        public static IOperator Create(string name)
        {
            if (!Factories.TryGetValue(name, out var factory))
                throw new ArgumentException($"Unknown operator '{name}'.", nameof(name));
            return factory();
        }

        /// <summary>
        /// 判断是否存在指定名称的算子。
        /// </summary>
        public static bool Exists(string name) => Factories.ContainsKey(name);
    }
}
