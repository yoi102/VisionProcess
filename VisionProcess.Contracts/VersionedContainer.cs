using MessagePack;

namespace VisionProcess.Contracts
{
    /// <summary>
    /// 带版本号的持久化容器，用于未来做迁移。
    /// </summary>
    [MessagePackObject]
    public class VersionedContainer
    {
        public const int CurrentVersion = 1;

        [Key(0)]
        public int Version { get; set; } = CurrentVersion;

        [Key(1)]
        public MainViewDto? MainView { get; set; }
    }
}
