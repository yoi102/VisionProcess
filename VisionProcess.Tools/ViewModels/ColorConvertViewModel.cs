using Newtonsoft.Json;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using VisionProcess.Lang.Strings;
using VisionProcess.Abstractions.ToolBase;
using VisionProcess.Tools.Extensions;
using VisionProcess.Tools.Models;

namespace VisionProcess.Tools.ViewModels
{
    public class ColorConvertViewModel : OperatorBase<ColorConvertInputs, ColorConvertOutputs, GraphicsEmpty>
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
            Outputs.Image ??= new Mat();
            Cv2.CvtColor(Inputs.Image, Outputs.Image, Inputs.ColorConversionCodes);
            Records[0].DisplayImage = Outputs.Image.ToFrozenBitmapSource();

            //to one channel ?
            //R*Weight
            //G*Weight
            //B*Weight
            //...............................................................
            message = Strings.Success;

            return true;
        }

        protected override IEnumerable<DefaultConnectorInfo> GetDefaultConnectors()
        {
            yield return new DefaultConnectorInfo(true, "Image", "Inputs.Image");
            yield return new DefaultConnectorInfo(false, "Image", "Outputs.Image");
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
                Records[^1].DisplayImage = Inputs.Image?.ToFrozenBitmapSource();
            }
        }
    }
}