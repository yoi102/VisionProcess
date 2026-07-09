using OpenCvSharp;
using VisionProcess.Abstractions.Attributes;
using VisionProcess.Abstractions.ToolBase;

namespace VisionProcess.Tools.Models
{
    public class ImageThresholdOutputs : OutputsBase
    {
        private Mat? image;

        public Mat? Image
        {
            get
            {
                return image;
            }
            internal set
            {
                if (image != value)
                {
                    image?.Dispose();
                    image = value;
                }
            }
        }

        protected override void ConfigureConnectors(ConnectorProviderBuilder builder)
        {
            builder.Add<Mat?>("Image", "Image", () => Image, null);
        }
    }
}