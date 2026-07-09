using OpenCvSharp;
using VisionProcess.Abstractions.Attributes;
using VisionProcess.Abstractions.ToolBase;

namespace VisionProcess.Tools.Models
{
    public class ImageThresholdInputs : InputsBase
    {
        private Mat? image;
        private double maximumValue = 255;
        private ThresholdTypes thresholdType = ThresholdTypes.Binary;
        private double thresholdValue = 50;

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

        /// <summary>
        /// maximum value to use with the THRESH_BINARY and THRESH_BINARY_INV thresholding types
        /// </summary>
        public double MaximumValue
        {
            get { return maximumValue; }
            set { SetProperty(ref maximumValue, value); }
        }

        /// <summary>
        /// thresholding type (see the details below).</param>
        /// </summary>
        public ThresholdTypes ThresholdType
        {
            get { return thresholdType; }
            set { SetProperty(ref thresholdType, value); }
        }

        /// <summary>
        /// threshold value
        /// </summary>
        public double ThresholdValue
        {
            get { return thresholdValue; }
            set { SetProperty(ref thresholdValue, value); }
        }

        protected override void ConfigureConnectors(ConnectorProviderBuilder builder)
        {
            builder.Add<Mat?>("Image", "Image", () => Image, v => Image = v);
            builder.Add<double>("MaximumValue", "MaximumValue", () => MaximumValue, v => MaximumValue = v);
            builder.Add<ThresholdTypes>("ThresholdType", "ThresholdType", () => ThresholdType, v => ThresholdType = v);
            builder.Add<double>("ThresholdValue", "ThresholdValue", () => ThresholdValue, v => ThresholdValue = v);
        }
    }
}