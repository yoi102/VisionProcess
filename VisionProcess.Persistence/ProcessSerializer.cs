using MessagePack;
using VisionProcess.Persistence;
using VisionProcess.Persistence.Migrations;

namespace VisionProcess.Persistence
{
    /// <summary>
    /// 使用 MessagePack 对 <see cref="VersionedContainer"/> 进行序列化与反序列化，
    /// 并在反序列化时执行迁移。
    /// </summary>
    public sealed class ProcessSerializer
    {
        private readonly MigrationRunner migrationRunner;

        public ProcessSerializer(MigrationRunner migrationRunner)
        {
            this.migrationRunner = migrationRunner;
        }

        /// <summary>
        /// 将 <see cref="VersionedContainer"/> 序列化为 MessagePack 字节数组。
        /// </summary>
        public byte[] Serialize(VersionedContainer container)
        {
            return MessagePackSerializer.Serialize(container);
        }

        /// <summary>
        /// 从 MessagePack 字节数组反序列化，并自动迁移到当前版本。
        /// </summary>
        public VersionedContainer Deserialize(byte[] data)
        {
            var container = MessagePackSerializer.Deserialize<VersionedContainer>(data);
            return migrationRunner.MigrateToCurrent(container);
        }

        /// <summary>
        /// 从 MessagePack 字节数组反序列化，不执行迁移。
        /// </summary>
        public VersionedContainer DeserializeRaw(byte[] data)
        {
            return MessagePackSerializer.Deserialize<VersionedContainer>(data);
        }
    }
}
