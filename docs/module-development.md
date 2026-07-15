# VisionProcess 模块接入

当前模块边界由两个注册对象组成：

- `OperatorDescriptor`：稳定 ID、显示名称、算子工厂、状态版本和状态编解码。
- `IOperatorViewModelRegistration`：算子类型到编辑器 ViewModel 的创建逻辑。

一个应用模块通过 `IOperatorModule` 一次性提供以上两组注册。核心菜单、算子工厂和持久化映射器只读取注册表，不再为新算子增加类型 `switch`。

## 最小模块

```csharp
public sealed class MyVisionModule : IOperatorModule
{
    public IReadOnlyList<OperatorDescriptor> Operators { get; } =
    [
        new OperatorDescriptor(
            id: "Company.EdgeDetect",
            displayName: "Edge Detect",
            operatorType: typeof(EdgeDetectOperator),
            factory: static () => new EdgeDetectOperator(),
            stateVersion: 1,
            serializeState: static op =>
                JsonSerializer.SerializeToUtf8Bytes(
                    new EdgeState(((EdgeDetectOperator)op).Threshold)),
            deserializeState: static (version, payload) =>
            {
                if (version != 1)
                    throw new NotSupportedException($"State version {version} is not supported.");

                var state = JsonSerializer.Deserialize<EdgeState>(payload)!;
                return new EdgeDetectOperator { Threshold = state.Threshold };
            })
    ];

    public IReadOnlyList<IOperatorViewModelRegistration> ViewModels { get; } =
    [
        new OperatorViewModelRegistration<EdgeDetectOperator>(
            static op => new EdgeDetectViewModel(op))
    ];

    private sealed record EdgeState(double Threshold);
}
```

在应用组合根注册模块：

```csharp
services.AddSingleton<IOperatorModule, MyVisionModule>();
```

## 兼容规则

- `OperatorDescriptor.Id` 是文件格式的一部分，发布后不要改名。
- 状态结构变化时递增 `stateVersion`，并在 `deserializeState` 中保留旧版本迁移逻辑。
- 算子输出 `Mat` 应为本次执行新建且由输出端持有；输入保存 `Mat` 时应克隆，避免重复执行后的复用或释放问题。
- 图执行传播继续以 `ProcessModel.Connections` 为唯一连接来源，不要在模块内部维护第二套连接关系。

旧 `.vproc` 的四种 MessagePack Union 算子仍可读取；新文件使用模块 ID + 版本化 payload。
