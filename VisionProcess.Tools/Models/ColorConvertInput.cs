using Newtonsoft.Json;
using OpenCvSharp;
using VisionProcess.Core.ToolBase;

namespace VisionProcess.Tools.Models
{
    public class ColorConvertInput : InputsBase
    {
        private ColorConversionCodes colorConversionCodes = ColorConversionCodes.BGR2GRAY;
        private Mat? image;

        public ColorConversionCodes ColorConversionCodes
        {
            get { return colorConversionCodes; }
            set { SetProperty(ref colorConversionCodes, value); }
        }

        [JsonIgnore]
        public Mat? Image
        {
            get
            {
                return image;
            }
            set
            {
                if (image != value)
                {
                    image?.Dispose();
                    image = value;
                    OnPropertyChanged();
                }
            }
        }

        [JsonProperty(nameof(Image))]
        public byte[]? ImageBytes
        {
            get { return Image?.ToBytes(); }
            set
            {
                if (value is not null)
                {
                    Image = Mat.FromArray(value);
                }
            }
        }
    }
}