using MessagePack;

namespace VisionProcess.Persistence.Operators
{
    /// <summary>
    /// 图片采集算子的持久化数据。
    /// </summary>
    [MessagePackObject]
    public class AcquireImageOperatorDto : OperatorDto
    {
        [Key(2)]
        public int CurrentIndex { get; set; }

        [Key(3)]
        public List<string> ImagePaths { get; set; } = new List<string>();
    }
}
