using MessagePack;

namespace VisionProcess.Contracts
{
    [MessagePackObject]
    public class OperationGroupDto : OperationDto
    {
        [Key(7)]
        public SizeDto GroupSize { get; set; }
    }
}
