using OpenCvSharp;
using VisionProcess.Core.ToolBase;

namespace VisionProcess.Tools.Models
{
    public class AcquireImageOutput : OutputsBase
    {
        private Mat? _image;

        public Mat? Image
        {
            get
            {
                return _image;
            }
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