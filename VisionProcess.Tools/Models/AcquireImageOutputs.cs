using VisionProcess.Abstractions.ToolBase;

namespace VisionProcess.Tools.Models
{
    public sealed class AcquireImageOutputs : ImageOutputsBase
    {
        private string fileName = string.Empty;
        private string filePath = string.Empty;
        private int frameCount;
        private int frameIndex;

        public string FilePath
        {
            get => filePath;
            internal set => SetProperty(ref filePath, value);
        }

        public string FileName
        {
            get => fileName;
            internal set => SetProperty(ref fileName, value);
        }

        public int FrameIndex
        {
            get => frameIndex;
            internal set => SetProperty(ref frameIndex, value);
        }

        public int FrameCount
        {
            get => frameCount;
            internal set => SetProperty(ref frameCount, value);
        }

        protected override void ConfigureAdditionalConnectors(ConnectorProviderBuilder builder)
        {
            builder.Add<string>("FilePath", "FilePath", () => FilePath, null);
            builder.Add<string>("FileName", "FileName", () => FileName, null);
            builder.Add<int>("FrameIndex", "FrameIndex", () => FrameIndex, null);
            builder.Add<int>("FrameCount", "FrameCount", () => FrameCount, null);
        }
    }
}
