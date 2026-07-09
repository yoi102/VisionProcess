using MessagePack;

namespace VisionProcess.Persistence
{
    [MessagePackObject]
    public class ConnectorDto
    {
        [Key(0)]
        public string Title { get; set; } = string.Empty;

        [Key(1)]
        public string ValueTypeName { get; set; } = string.Empty;

        [Key(2)]
        public string ValuePath { get; set; } = string.Empty;

        [Key(3)]
        public bool IsInput { get; set; }

        [Key(4)]
        public Guid OwnerId { get; set; }

        [Key(5)]
        public bool IsConnected { get; set; }
    }
}
