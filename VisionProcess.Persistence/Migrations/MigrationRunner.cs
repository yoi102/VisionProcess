using VisionProcess.Contracts;

namespace VisionProcess.Persistence.Migrations
{
    /// <summary>
    /// 按版本号顺序执行迁移。
    /// </summary>
    public sealed class MigrationRunner
    {
        private readonly IReadOnlyDictionary<int, IMigration> migrations;

        public MigrationRunner(IEnumerable<IMigration> migrations)
        {
            this.migrations = migrations.ToDictionary(m => m.FromVersion);
        }

        /// <summary>
        /// 将容器升级到当前版本。
        /// </summary>
        public VersionedContainer MigrateToCurrent(VersionedContainer source)
        {
            var current = source;
            while (current.Version < VersionedContainer.CurrentVersion)
            {
                if (!migrations.TryGetValue(current.Version, out var migration))
                {
                    throw new NotSupportedException($"No migration registered from version {current.Version}.");
                }

                current = migration.Upgrade(current);
            }

            return current;
        }
    }
}
