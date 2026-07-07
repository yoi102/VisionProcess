using MessagePack;

namespace VisionProcess.Contracts
{
    [MessagePackObject]
    public struct SizeDto
    {
        [Key(0)]
        public double Width { get; set; }

        [Key(1)]
        public double Height { get; set; }
    }
}
