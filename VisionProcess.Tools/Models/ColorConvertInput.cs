using OpenCvSharp;
using VisionProcess.Core.ToolBase;

namespace VisionProcess.Tools.Models
{
    public class ColorConvertInput : InputsBase
    {
        private ColorConversionCodes _colorConversionCodes = ColorConversionCodes.BGR2GRAY;
        private Mat? _image;

        public ColorConversionCodes ColorConversionCodes
        {
            get { return _colorConversionCodes; }
            set { SetProperty(ref _colorConversionCodes, value); }
        }

        public Mat? Image
        {
            get
            {
                return _image;
            }
            set
            {
                if (_image != value)
                {
                    _image?.Dispose();
                    _image = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}