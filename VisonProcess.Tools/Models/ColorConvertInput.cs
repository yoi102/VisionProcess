using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisonProcess.Core.ToolBase;

namespace VisonProcess.Tools.Models
{
    public class ColorConvertInput: InputsBase
    {

        private Mat? _image;

        public Mat? Image
        {
            get { return _image; }
            set
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
