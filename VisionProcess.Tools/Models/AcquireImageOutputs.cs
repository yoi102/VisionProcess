using Newtonsoft.Json;
using OpenCvSharp;
using VisionProcess.Abstractions.Attributes;
using VisionProcess.Abstractions.ToolBase;

namespace VisionProcess.Tools.Models
{
    public class AcquireImageOutputs : OutputsBase
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

        /// <summary>
        /// 用于持久化的图像数据。以 PNG 格式编码后存储，避免直接序列化 <see cref="Mat"/>。
        /// </summary>
        [ThresholdIgnore]
        [JsonProperty(nameof(Image))]
        public byte[]? ImageBytes
        {
            get
            {
                if (Image is null || Image.Empty())
                {
                    return null;
                }

                Cv2.ImEncode(".png", Image, out byte[] buffer);
                return buffer;
            }
            set
            {
                if (value is null || value.Length == 0)
                {
                    Image = null;
                    return;
                }

                Image = Cv2.ImDecode(value, ImreadModes.Unchanged);
            }
        }

        protected override void ConfigureConnectors(ConnectorProviderBuilder builder)
        {
            builder.Add<Mat?>("Image", "Image", () => Image, null);
        }
    }
}