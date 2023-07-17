using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.Linq;
using System.Windows.Media.Imaging;
using VisionProcess.Core.Attributes;
using VisionProcess.Core.Strings;
using VisionProcess.Core.ToolBase;
using VisionProcess.Tools.Models;

namespace VisionProcess.Tools.ViewModels
{
    [DefaultToolConnector(true, "Image", "Inputs.Image")]
    [DefaultToolConnector(false, "Image", "Outputs.Image")]
    public class ColorConvertViewModel : OperationBase<ColorConvertInput, ColorConvertOutput, GraphicsEmpty>
    {
        public ColorConvertViewModel() : base()
        {
            Init();
            Inputs.PropertyChanged += Inputs_PropertyChanged;
        }

        private void Inputs_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName?.Equals(nameof(Inputs.Image)) == true)
            {
                Records[^1].DisplayImage = Inputs.Image?.ToBitmapSource();
            }
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

        private void Init()
        {
            Records.Add(new() { Title = Strings.OutputImage });
            Records.Add(new() { Title = Strings.InputImage });

        }
    }
}