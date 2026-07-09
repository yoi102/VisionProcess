using MessagePack;

namespace VisionProcess.Persistence
{
    [MessagePackObject]
    public struct PointDto
    {
        [Key(0)]
        public double X { get; set; }

        [Key(1)]
        public double Y { get; set; }
    }
}
