using MessagePack;
using OpenCvSharp;

namespace VisionProcess.Persistence.Operators
{
    /// <summary>
    /// 图像阈值算子的持久化数据。
    /// </summary>
    [MessagePackObject]
    public class ImageThresholdOperatorDto : OperatorDto
    {
        [Key(2)]
        public double MaximumValue { get; set; }

        [Key(3)]
        public ThresholdTypes ThresholdType { get; set; }

        [Key(4)]
        public double ThresholdValue { get; set; }
    }
}
