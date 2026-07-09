using MessagePack;

namespace VisionProcess.Persistence
{
    [MessagePackObject]
    public class EditorDto
    {
        [Key(0)]
        public Guid Id { get; set; }

        [Key(1)]
        public string? Name { get; set; }

        [Key(2)]
        public ProcessDto? Process { get; set; }
    }
}
