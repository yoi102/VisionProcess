using MessagePack;
using VisionProcess.Persistence.Operators;

namespace VisionProcess.Persistence
{
    [MessagePackObject]
    public class OperationDto
    {
        [Key(0)]
        public Guid Id { get; set; }

        [Key(1)]
        public OperatorDto? Operator { get; set; }

        [Key(2)]
        public List<ConnectorDto> Inputs { get; set; } = new List<ConnectorDto>();

        [Key(3)]
        public List<ConnectorDto> Outputs { get; set; } = new List<ConnectorDto>();

        [Key(4)]
        public PointDto Location { get; set; }

        [Key(5)]
        public bool IsSelected { get; set; }

        [Key(6)]
        public SizeDto Size { get; set; }

        [Key(8)]
        public List<RecordDto> Records { get; set; } = new List<RecordDto>();

        [Key(9)]
        public OperatorStateDto? OperatorState { get; set; }
    }
}
