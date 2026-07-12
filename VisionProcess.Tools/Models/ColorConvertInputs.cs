using OpenCvSharp;
using VisionProcess.Abstractions.ToolBase;

namespace VisionProcess.Tools.Models
{
    public class ColorConvertInputs : InputsBase
    {
        private ColorConvertType colorConversionCodes = ColorConvertType.BGR2GRAY;
        private Mat? image;

        public ColorConvertType ColorConversionCodes
        {
            get { return colorConversionCodes; }
            set { SetProperty(ref colorConversionCodes, value); }
        }

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
                    var ownedImage = value?.Clone();
                    image?.Dispose();
                    image = ownedImage;
                    OnPropertyChanged();
                }
            }
        }

        protected override void ConfigureConnectors(ConnectorProviderBuilder builder)
        {
            builder.Add<Mat?>("Image", "Image", () => Image, v => Image = v);
            builder.Add<ColorConvertType>("ColorConversionCodes", "ColorConversionCodes", () => ColorConversionCodes, v => ColorConversionCodes = v);
        }
    }
}
