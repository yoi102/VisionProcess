using System.IO;
using VisionProcess.Contracts;

namespace VisionProcess.Persistence
{
    /// <summary>
    /// 基于文件的高阶持久化服务，负责 <see cref="VersionedContainer"/> 与磁盘文件之间的读写。
    /// </summary>
    public sealed class ProcessPersistenceService
    {
        private readonly ProcessSerializer serializer;
        private readonly string? filePath;

        public ProcessPersistenceService(ProcessSerializer serializer, string? filePath = null)
        {
            this.serializer = serializer;
            this.filePath = filePath;
        }

        /// <summary>
        /// 将容器保存到默认文件；未指定默认路径时抛出异常。
        /// </summary>
        public void Save(VersionedContainer container)
        {
            if (string.IsNullOrEmpty(filePath))
                throw new InvalidOperationException("No default file path was configured for this persistence service.");

            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var bytes = serializer.Serialize(container);
            File.WriteAllBytes(filePath, bytes);
        }

        /// <summary>
        /// 从默认文件加载容器；文件不存在时返回 <c>null</c>。
        /// </summary>
        public VersionedContainer? Load()
        {
            if (string.IsNullOrEmpty(filePath))
                throw new InvalidOperationException("No default file path was configured for this persistence service.");

            if (!File.Exists(filePath))
            {
                return null;
            }

            var bytes = File.ReadAllBytes(filePath);
            return serializer.Deserialize(bytes);
        }

        /// <summary>
        /// 将容器保存到指定路径的文件。
        /// </summary>
        public void SaveToFile(VersionedContainer container, string path)
        {
            var directory = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var bytes = serializer.Serialize(container);
            File.WriteAllBytes(path, bytes);
        }

        /// <summary>
        /// 从指定路径的文件加载容器；文件不存在时返回 <c>null</c>。
        /// </summary>
        public VersionedContainer? LoadFromFile(string path)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            var bytes = File.ReadAllBytes(path);
            return serializer.Deserialize(bytes);
        }
    }
}
