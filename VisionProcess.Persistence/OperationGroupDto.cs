using MessagePack;

namespace VisionProcess.Persistence
{
    [MessagePackObject]
    public class OperationGroupDto : OperationDto
    {
        [Key(7)]
        public SizeDto GroupSize { get; set; }
    }
}
