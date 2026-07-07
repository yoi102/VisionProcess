using Newtonsoft.Json;
using OpenCvSharp;
using VisionProcess.Abstractions.Attributes;
using VisionProcess.Abstractions.ToolBase;

namespace VisionProcess.Tools.Models
{
    public class ColorConvertOutputs : OutputsBase
    {
        private Mat? image;

        [JsonIgnore]
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
        [ThresholdIgnore]
        [JsonProperty(nameof(Image))]
        public byte[]? ImageBytes
        {
            get { return Image?.ToBytes(); }
            set
            {
                if (value is not null)
                {
                    Image = Cv2.ImDecode(value, ImreadModes.Unchanged);
                }
            }
        }

        protected override void ConfigureConnectors(ConnectorProviderBuilder builder)
        {
            builder.Add<Mat?>("Image", "Image", () => Image, null);
        }
    }
}