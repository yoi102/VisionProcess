using OpenCvSharp;
using VisonProcess.Core.ToolBase;

namespace VisonProcess.Tools.Models
{
    public class ThresholdInputs : InputsBase
    {
        private Mat? _image;
        private double _maxValue = 255;
        private double _threshold = 50;
        private ThresholdTypes _thresholdType = ThresholdTypes.Binary;

        public Mat? Image
        {
            get { return _image; }
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

        public double MaxValue
        {
            get { return _maxValue; }
            set { SetProperty(ref _maxValue, value); }
        }

        public double Threshold
        {
            get { return _threshold; }
            set { SetProperty(ref _threshold, value); }
        }

        public ThresholdTypes ThresholdType
        {
            get { return _thresholdType; }
            set { SetProperty(ref _thresholdType, value); }
        }
    }
}