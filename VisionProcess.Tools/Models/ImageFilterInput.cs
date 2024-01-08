using Newtonsoft.Json;
using OpenCvSharp;
using System.ComponentModel;
using VisionProcess.Core.Attributes;
using VisionProcess.Core.Converters;
using VisionProcess.Core.Strings;
using VisionProcess.Core.ToolBase;

namespace VisionProcess.Tools.Models
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum FilterTypes
    {
        [LocalizedDescription("NormalizedBox", typeof(Strings))]
        NormalizedBox,

        [LocalizedDescription("Median", typeof(Strings))]
        Median,

        [LocalizedDescription("Gaussian", typeof(Strings))]
        Gaussian,

        [LocalizedDescription("Bilateral", typeof(Strings))]
        Bilateral,
    }

    public class ImageFilterInput : InputsBase
    {
        private BorderTypes borderType = BorderTypes.Default;
        private FilterTypes filterType = FilterTypes.Median;
        private Mat? image;

        private int kernelHeight = 3;

        private int kernelWidth = 3;

        /// <summary>
        /// pixel extrapolation method
        /// </summary>
        public BorderTypes BorderType
        {
            get { return borderType; }
            set { SetProperty(ref borderType, value); }
        }

        public FilterTypes FilterType
        {
            get { return filterType; }
            set { SetProperty(ref filterType, value); }
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
                    Image = Cv2.ImDecode(value, ImreadModes.Unchanged);
                }
            }
        }

        public int KernelHeight
        {
            get { return kernelHeight; }
            set { SetProperty(ref kernelHeight, value); }
        }

        public int KernelWidth
        {
            get { return kernelWidth; }
            set { SetProperty(ref kernelWidth, value); }
        }

        #region Median

        private int _kernelSize = 3;

        public int KernelSize
        {
            get { return _kernelSize; }
            set { SetProperty(ref _kernelSize, value); }
        }

        #endregion Median

        #region Gaussian

        private double _sigmaX = 3;
        private double _sigmaY = 0;

        /// <summary>
        /// Gaussian kernel standard deviation in X direction.
        /// </summary>
        public double SigmaX
        {
            get { return _sigmaX; }
            set { SetProperty(ref _sigmaX, value); }
        }

        /// <summary>
        /// Gaussian kernel standard deviation in Y direction; if sigmaY is zero, it is set to be equal to sigmaX,
        /// if both sigmas are zeros, they are computed from ksize.width and ksize.height,
        /// respectively (see getGaussianKernel() for details); to fully control the result
        /// regardless of possible future modifications of all this semantics, it is recommended to specify all of ksize, sigmaX, and sigmaY.
        /// </summary>
        public double SigmaY
        {
            get { return _sigmaY; }
            set { SetProperty(ref _sigmaY, value); }
        }

        #endregion Gaussian

        #region BilateralFilter

        private int _diameter = 1;
        private double _sigmaColor = 1;

        private double _sigmaSpace = 1;

        /// <summary>
        /// The diameter of each pixel neighborhood, that is used during filtering.
        /// If it is non-positive, it's computed from sigmaSpace
        /// </summary>
        public int Diameter
        {
            get { return _diameter; }
            set { SetProperty(ref _diameter, value); }
        }

        /// <summary>
        /// Filter sigma in the color space.
        /// Larger value of the parameter means that farther colors within the pixel neighborhood
        /// will be mixed together, resulting in larger areas of semi-equal color
        /// </summary>
        public double SigmaColor
        {
            get { return _sigmaColor; }
            set { SetProperty(ref _sigmaColor, value); }
        }

        /// <summary>
        /// Filter sigma in the coordinate space.
        /// Larger value of the parameter means that farther pixels will influence each other
        /// (as long as their colors are close enough; see sigmaColor). Then d>0 , it specifies
        /// the neighborhood size regardless of sigmaSpace, otherwise d is proportional to sigmaSpace
        /// </summary>
        public double SigmaSpace
        {
            get { return _sigmaSpace; }
            set { SetProperty(ref _sigmaSpace, value); }
        }

        #endregion BilateralFilter
    }
}