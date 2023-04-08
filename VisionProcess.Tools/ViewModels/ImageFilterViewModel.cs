using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using VisionProcess.Core.Attributes;
using VisionProcess.Core.Strings;
using VisionProcess.Core.ToolBase;
using VisionProcess.Tools.Models;

namespace VisionProcess.Tools.ViewModels
{
    [DefaultToolConnector(true, "Image", "Inputs.Image")]
    [DefaultToolConnector(false, "Image", "Outputs.Image")]
    public class ImageFilterViewModel : OperationBase<ImageFilterInput, ImageFilterOutput, GraphicsBase>
    {
        public ImageFilterViewModel() : base()
        {
            Init();
        }

        protected override bool InternalExecute(out string message)
        {
            if (Inputs.Image is null)
            {
                message = "Input image can not be null";
                return false;
            }
            Outputs.Image ??= new Mat();
            switch (Inputs.FilterType)
            {
                case FilterTypes.NormalizedBox:
                    Cv2.Blur(Inputs.Image, Outputs.Image, new Size(Inputs.KernelWidth, Inputs.KernelHeight), borderType: Inputs.BorderType);
                    break;

                case FilterTypes.Median:
                    Cv2.MedianBlur(Inputs.Image, Outputs.Image, Inputs.KernelSize);
                    break;

                case FilterTypes.Gaussian:
                    Cv2.GaussianBlur(Inputs.Image, Outputs.Image, new Size(Inputs.KernelWidth, Inputs.KernelHeight), Inputs.SigmaX, Inputs.SigmaY, borderType: Inputs.BorderType);
                    break;

                case FilterTypes.Bilateral:
                    Cv2.BilateralFilter(Inputs.Image, Outputs.Image, Inputs.Diameter, Inputs.SigmaColor, Inputs.SigmaSpace, borderType: Inputs.BorderType);
                    break;

                default:
                    break;
            }

            Records[0].DisplayImage = Outputs.Image.ToBitmapSource();
            message = Strings.Success;
            return true;
        }

        private void Init()
        {
            Records.Add(new() { Title = Strings.OutputImage });
        }
    }
}