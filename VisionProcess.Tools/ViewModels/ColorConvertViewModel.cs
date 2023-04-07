using OpenCvSharp.WpfExtensions;
using VisionProcess.Core.Attributes;
using VisionProcess.Core.Strings;
using VisionProcess.Core.ToolBase;
using VisionProcess.Tools.Models;

namespace VisionProcess.Tools.ViewModels
{
    [DefaultToolConnector(true, "Image", "Inputs.Image")]
    [DefaultToolConnector(false, "Image", "Outputs.Image")]
    public class ColorConvertViewModel : OperationBase<ColorConvertInput, ColorConvertOutput, GraphicsBase>
    {
        public ColorConvertViewModel() : base()
        {
            Init();
        }

        private void Init()
        {
            Records.Add(new() { Title = Strings.OutputImage });
        }

        protected override bool InternalExecute(out string message)
        {
            if (Inputs.Image is null)
            {
                message = "Input image can not be null";
                return false;
            }
            //Cv2.CvtColor(Inputs.Image, Outputs.Image, ColorConversionCodes.RGB2BGRA);//RGB2BGRA? BRG2BGRA?
            Outputs.Image = Inputs.Image.CvtColor(Inputs.ColorConversionCodes);
            Records[0].DisplayImage = Outputs.Image.ToBitmapSource();

            //to one channel ?
            //R*Weight
            //G*Weight
            //B*Weight
            //...............................................................
            message = Strings.Success;

            return true;
        }
    }
}