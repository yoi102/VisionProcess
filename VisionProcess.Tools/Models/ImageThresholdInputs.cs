using OpenCvSharp;
using VisionProcess.Core.ToolBase;

namespace VisionProcess.Tools.Models
{
    public class ImageThresholdInputs : InputsBase
    {
        private Mat? _image;
        private double _maximumValue = 255;
        private ThresholdTypes _thresholdType = ThresholdTypes.Binary;
        private double _thresholdValue = 50;

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

        /// <summary>
        /// maximum value to use with the THRESH_BINARY and THRESH_BINARY_INV thresholding types
        /// </summary>
        public double MaximumValue
        {
            get { return _maximumValue; }
            set { SetProperty(ref _maximumValue, value); }
        }

        /// <summary>
        /// thresholding type (see the details below).</param>
        /// </summary>
        public ThresholdTypes ThresholdType
        {
            get { return _thresholdType; }
            set { SetProperty(ref _thresholdType, value); }
        }

        /// <summary>
        /// threshold value
        /// </summary>
        public double ThresholdValue
        {
            get { return _thresholdValue; }
            set { SetProperty(ref _thresholdValue, value); }
        }
    }
}