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
    public class ImageFilterViewModel : OperationBase<ImageFilterInput, ImageFilterOutput, GraphicsEmpty>
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
                    Outputs.Image = Inputs.Image.Blur(new Size(Inputs.KernelWidth, Inputs.KernelHeight), borderType: Inputs.BorderType);
                    //Cv2.Blur(Inputs.Image, Outputs.Image, new Size(Inputs.KernelWidth, Inputs.KernelHeight), borderType: Inputs.BorderType);
                    break;

                case FilterTypes.Median:
                    Outputs.Image = Inputs.Image.MedianBlur(Inputs.KernelSize);
                    //Cv2.MedianBlur(Inputs.Image, Outputs.Image, Inputs.KernelSize);
                    break;

                case FilterTypes.Gaussian:
                    Outputs.Image = Inputs.Image.GaussianBlur(new Size(Inputs.KernelWidth, Inputs.KernelHeight), Inputs.SigmaX, Inputs.SigmaY, borderType: Inputs.BorderType);
                    //Cv2.GaussianBlur(Inputs.Image, Outputs.Image, new Size(Inputs.KernelWidth, Inputs.KernelHeight), Inputs.SigmaX, Inputs.SigmaY, borderType: Inputs.BorderType);
                    break;

                case FilterTypes.Bilateral:
                    Outputs.Image = Inputs.Image.BilateralFilter(Inputs.Diameter, Inputs.SigmaColor, Inputs.SigmaSpace, borderType: Inputs.BorderType);
                    //Cv2.BilateralFilter(Inputs.Image, Outputs.Image, Inputs.Diameter, Inputs.SigmaColor, Inputs.SigmaSpace, borderType: Inputs.BorderType);
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