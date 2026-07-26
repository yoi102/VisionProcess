using VisionProcess.Abstractions.ToolBase;

namespace VisionProcess.Tools.Models
{
    public sealed class ImageThresholdOutputs : ImageOutputsBase
    {
        private double appliedThreshold;
        private long nonZeroPixelCount;

        public double AppliedThreshold
        {
            get => appliedThreshold;
            internal set => SetProperty(ref appliedThreshold, value);
        }

        public long NonZeroPixelCount
        {
            get => nonZeroPixelCount;
            internal set => SetProperty(ref nonZeroPixelCount, value);
        }

        protected override void ConfigureAdditionalConnectors(ConnectorProviderBuilder builder)
        {
            builder.Add<double>(
                "AppliedThreshold",
                "AppliedThreshold",
                () => AppliedThreshold,
                null);
            builder.Add<long>(
                "NonZeroPixelCount",
                "NonZeroPixelCount",
                () => NonZeroPixelCount,
                null);
        }
    }
}
