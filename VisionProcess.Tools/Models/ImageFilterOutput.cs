using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisionProcess.Core.ToolBase;

namespace VisionProcess.Tools.Models
{
    public class ImageFilterOutput : OutputsBase
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