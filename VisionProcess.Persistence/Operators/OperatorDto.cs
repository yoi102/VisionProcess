using MessagePack;

namespace VisionProcess.Persistence.Operators
{
    /// <summary>
    /// 算子持久化数据基类。使用 MessagePack Union 实现多态序列化。
    /// </summary>
    [MessagePackObject]
    [Union(0, typeof(AcquireImageOperatorDto))]
    [Union(1, typeof(ColorConvertOperatorDto))]
    [Union(2, typeof(ImageFilterOperatorDto))]
    [Union(3, typeof(ImageThresholdOperatorDto))]
    public abstract class OperatorDto
    {
        [Key(0)]
        public string? Name { get; set; }

        [Key(1)]
        public RunStatusDto RunStatus { get; set; } = new RunStatusDto();
    }
}
