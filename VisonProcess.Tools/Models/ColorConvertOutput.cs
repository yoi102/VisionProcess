using OpenCvSharp;
using VisonProcess.Core.ToolBase;

namespace VisonProcess.Tools.Models
{
    public class ColorConvertOutput : OutputsBase
    {
        private Mat? _image;

        public Mat? Image
        {
            get { return _image; }
            internal set
            {
                if (_image != value)
                {
                    _image?.Dispose();
                    _image = value;
                }
            }
        }
    }
}