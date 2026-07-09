using MessagePack;

namespace VisionProcess.Persistence
{
    /// <summary>
    /// 算子运行时记录的持久化数据。
    /// </summary>
    [MessagePackObject]
    public class RecordDto
    {
        [Key(0)]
        public string? Title { get; set; }

        [Key(1)]
        public byte[]? ImageData { get; set; }
    }
}
