namespace VisionProcess.Abstractions.ToolBase;

/// <summary>
/// Describes one operator module available to the platform.
/// The id is a stable persisted identifier; the display name is UI-facing.
/// </summary>
public sealed class OperatorDescriptor
{
    private readonly Func<IOperator> factory;
    private readonly Func<IOperator, byte[]>? serializeState;
    private readonly Func<int, byte[], IOperator>? deserializeState;

    public OperatorDescriptor(
        string id,
        string displayName,
        Type operatorType,
        Func<IOperator> factory,
        int stateVersion = 0,
        Func<IOperator, byte[]>? serializeState = null,
        Func<int, byte[], IOperator>? deserializeState = null)
    {
        if (string.IsNullOrWhiteSpace(id))
            throw new ArgumentException("An operator id is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(displayName))
            throw new ArgumentException("An operator display name is required.", nameof(displayName));
        if (!typeof(IOperator).IsAssignableFrom(operatorType))
            throw new ArgumentException($"'{operatorType}' is not an operator type.", nameof(operatorType));
        if ((serializeState is null) != (deserializeState is null))
        {
            throw new ArgumentException(
                "State serialization and deserialization must be registered together.");
        }
        if (stateVersion < 0)
            throw new ArgumentOutOfRangeException(nameof(stateVersion));

        Id = id;
        DisplayName = displayName;
        OperatorType = operatorType;
        this.factory = factory ?? throw new ArgumentNullException(nameof(factory));
        StateVersion = stateVersion;
        this.serializeState = serializeState;
        this.deserializeState = deserializeState;
    }

    public string Id { get; }

    public string DisplayName { get; }

    public Type OperatorType { get; }

    public int StateVersion { get; }

    public bool SupportsPersistence => serializeState is not null;

    public IOperator CreateOperator()
    {
        var instance = factory();
        if (!OperatorType.IsInstanceOfType(instance))
        {
            throw new InvalidOperationException(
                $"Operator factory '{Id}' returned '{instance.GetType()}', expected '{OperatorType}'.");
        }

        return instance;
    }

    public byte[] SerializeState(IOperator @operator)
    {
        ArgumentNullException.ThrowIfNull(@operator);
        if (!OperatorType.IsInstanceOfType(@operator))
        {
            throw new ArgumentException(
                $"Expected operator '{OperatorType}', received '{@operator.GetType()}'.",
                nameof(@operator));
        }

        return serializeState?.Invoke(@operator)
            ?? throw new NotSupportedException($"Operator '{Id}' has no state serializer.");
    }

    public IOperator DeserializeState(int stateVersion, byte[] payload)
    {
        ArgumentNullException.ThrowIfNull(payload);
        var instance = deserializeState?.Invoke(stateVersion, payload)
            ?? throw new NotSupportedException($"Operator '{Id}' has no state deserializer.");
        if (!OperatorType.IsInstanceOfType(instance))
        {
            throw new InvalidOperationException(
                $"Operator state deserializer '{Id}' returned '{instance.GetType()}', expected '{OperatorType}'.");
        }

        return instance;
    }
}
