using VisionProcess.Persistence;

namespace VisionProcess.Persistence.Migrations
{
    /// <summary>
    /// 持久化数据迁移接口。每个实现处理一个版本到下一个版本的升级。
    /// </summary>
    public interface IMigration
    {
        /// <summary>
        /// 源版本号（从该版本升级）。
        /// </summary>
        int FromVersion { get; }

        /// <summary>
        /// 升级后的版本号。
        /// </summary>
        int ToVersion { get; }

        /// <summary>
        /// 将数据从 <see cref="FromVersion"/> 升级到 <see cref="ToVersion"/>。
        /// </summary>
        VersionedContainer Upgrade(VersionedContainer source);
    }
}
