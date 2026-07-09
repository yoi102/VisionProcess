using MessagePack;

namespace VisionProcess.Persistence
{
    /// <summary>
    /// 算子运行状态的持久化数据。不保存异常对象。
    /// </summary>
    [MessagePackObject]
    public class RunStatusDto
    {
        [Key(0)]
        public DateTime LastTime { get; set; }

        [Key(1)]
        public string Message { get; set; } = string.Empty;

        [Key(2)]
        public double ProcessingTime { get; set; }

        [Key(3)]
        public bool Result { get; set; }
    }
}
