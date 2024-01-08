using Newtonsoft.Json;
using OpenCvSharp.WpfExtensions;
using System.Collections.ObjectModel;
using System.Drawing;
using VisionProcess.Core.Attributes;
using VisionProcess.Core.Strings;
using VisionProcess.Core.ToolBase;
using VisionProcess.Tools.Models;

namespace VisionProcess.Tools.ViewModels
{
    [DefaultToolConnector(true, "Image", "Inputs.Image")]
    [DefaultToolConnector(false, "Image", "Outputs.Image")]
    public class ColorConvertViewModel : OperationBase<ColorConvertInputs, ColorConvertOutputs, GraphicsEmpty>
    {
        public ColorConvertViewModel() : base()
        {
            Init();
            Inputs.PropertyChanged += Inputs_PropertyChanged;
        }

        [JsonConstructor]
        public ColorConvertViewModel(ColorConvertInputs inputs, ColorConvertOutputs outputs, GraphicsEmpty graphics, RunStatus runStatus) 
            : base(inputs, outputs, graphics, runStatus)
        {
            Inputs.PropertyChanged += Inputs_PropertyChanged;
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

        private void Inputs_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName?.Equals(nameof(Inputs.Image)) == true)
            {
                Records[^1].DisplayImage = Inputs.Image?.ToBitmapSource();
            }
        }
    }
}