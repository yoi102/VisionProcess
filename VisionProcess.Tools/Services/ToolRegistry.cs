using System;
using System.Collections.Generic;
using System.Linq;
using VisionProcess.Abstractions.ToolBase;
using VisionProcess.Tools.Operators;

namespace VisionProcess.Tools.Services
{
    /// <summary>
    /// 算子注册表。所有可被节点编辑器使用的算子类型必须在此显式注册。
    /// 这替代了原先通过反射扫描程序集发现算子的方式。
    /// </summary>
    public static class ToolRegistry
    {
        /// <summary>
        /// 所有已注册的算子类型。
        /// </summary>
        public static IReadOnlyList<Type> ToolTypes { get; } = new List<Type>
        {
            typeof(AcquireImageOperator),
            typeof(ImageFilterOperator),
            typeof(ImageThresholdOperator),
            typeof(ColorConvertOperator),
        }.AsReadOnly();

        /// <summary>
        /// 按类型全名索引的算子类型字典。
        /// </summary>
        public static IReadOnlyDictionary<string, Type> ToolTypesByName { get; } =
            ToolTypes.ToDictionary(t => t.Name, t => t, StringComparer.Ordinal);

        /// <summary>
        /// 将算子类型转换为菜单显示名称。
        /// </summary>
        public static string GetToolName(Type type) => type.Name.Replace("Operator", string.Empty);

        /// <summary>
        /// 根据菜单名称获取算子类型。
        /// </summary>
        public static Type? GetToolType(string name)
        {
            var typeName = name + "Operator";
            ToolTypesByName.TryGetValue(typeName, out var type);
            return type;
        }
    }
}
