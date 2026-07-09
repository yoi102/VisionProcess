using MessagePack;
using OpenCvSharp;
using VisionProcess.Tools.Models;

namespace VisionProcess.Persistence.Operators
{
    /// <summary>
    /// 图像滤波算子的持久化数据。
    /// </summary>
    [MessagePackObject]
    public class ImageFilterOperatorDto : OperatorDto
    {
        [Key(2)]
        public BorderTypes BorderType { get; set; }

        [Key(3)]
        public FilterTypes FilterType { get; set; }

        [Key(4)]
        public int KernelHeight { get; set; }

        [Key(5)]
        public int KernelWidth { get; set; }

        [Key(6)]
        public int KernelSize { get; set; }

        [Key(7)]
        public double SigmaX { get; set; }

        [Key(8)]
        public double SigmaY { get; set; }

        [Key(9)]
        public int Diameter { get; set; }

        [Key(10)]
        public double SigmaColor { get; set; }

        [Key(11)]
        public double SigmaSpace { get; set; }
    }
}
