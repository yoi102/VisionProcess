using MessagePack;

namespace VisionProcess.Contracts
{
    [MessagePackObject]
    public class ConnectionDto
    {
        [Key(0)]
        public ConnectorDto? Input { get; set; }

        [Key(1)]
        public ConnectorDto? Output { get; set; }
    }
}
