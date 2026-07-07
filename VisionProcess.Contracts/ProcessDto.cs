using MessagePack;

namespace VisionProcess.Contracts
{
    [MessagePackObject]
    public class ProcessDto
    {
        [Key(0)]
        public List<OperationDto> Operations { get; set; } = new List<OperationDto>();

        [Key(1)]
        public List<ConnectionDto> Connections { get; set; } = new List<ConnectionDto>();

        [Key(2)]
        public List<Guid> SelectedOperationIds { get; set; } = new List<Guid>();

        [Key(3)]
        public Guid? SelectedOperationId { get; set; }
    }
}
