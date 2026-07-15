namespace VisionProcess.Abstractions.ToolBase;

public sealed class OperatorCatalog : IOperatorCatalog
{
    private readonly IReadOnlyDictionary<string, OperatorDescriptor> operatorsById;
    private readonly IReadOnlyDictionary<Type, OperatorDescriptor> operatorsByType;

    public OperatorCatalog(IEnumerable<OperatorDescriptor> operators)
    {
        ArgumentNullException.ThrowIfNull(operators);

        var items = operators.ToArray();
        var duplicate = items
            .GroupBy(item => item.Id, StringComparer.Ordinal)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicate is not null)
        {
            throw new ArgumentException($"Duplicate operator id '{duplicate.Key}'.", nameof(operators));
        }

        var duplicateType = items
            .GroupBy(item => item.OperatorType)
            .FirstOrDefault(group => group.Count() > 1);
        if (duplicateType is not null)
        {
            throw new ArgumentException(
                $"Duplicate operator type '{duplicateType.Key}'.",
                nameof(operators));
        }

        Operators = Array.AsReadOnly(items);
        operatorsById = items.ToDictionary(item => item.Id, StringComparer.Ordinal);
        operatorsByType = items.ToDictionary(item => item.OperatorType);
    }

    public IReadOnlyList<OperatorDescriptor> Operators { get; }

    public OperatorDescriptor Get(string id)
        => TryGet(id, out var descriptor)
            ? descriptor
            : throw new KeyNotFoundException($"Operator '{id}' is not registered.");

    public bool TryGet(string id, out OperatorDescriptor descriptor)
        => operatorsById.TryGetValue(id, out descriptor!);

    public bool TryGet(Type operatorType, out OperatorDescriptor descriptor)
        => operatorsByType.TryGetValue(operatorType, out descriptor!);
}
