using MessagePack;

namespace VisionProcess.Persistence.Operators;

/// <summary>
/// Module-neutral persisted operator state. The owning module defines the payload format.
/// </summary>
[MessagePackObject]
public sealed class OperatorStateDto
{
    [Key(0)]
    public string OperatorId { get; set; } = string.Empty;

    [Key(1)]
    public int StateVersion { get; set; }

    [Key(2)]
    public byte[] Payload { get; set; } = [];

    [Key(3)]
    public string? Name { get; set; }

    [Key(4)]
    public RunStatusDto RunStatus { get; set; } = new();
}
