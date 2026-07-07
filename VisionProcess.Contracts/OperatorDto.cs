using MessagePack;

namespace VisionProcess.Contracts
{
    /// <summary>
    /// 算子持久化数据。Kind 用于显式标识算子类型，Json 保存算子内部状态。
    /// </summary>
    [MessagePackObject]
    public class OperatorDto
    {
        [Key(0)]
        public string Kind { get; set; } = string.Empty;

        [Key(1)]
        public string Json { get; set; } = string.Empty;
    }
}
