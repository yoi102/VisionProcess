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
    [DefaultToolConnector(true, "ThresholdValue", "Inputs.ThresholdValue")]
    [DefaultToolConnector(true, "MaximumValue", "Inputs.MaximumValue")]
    [DefaultToolConnector(false, "Image", "Outputs.Image")]
    public class ImageThresholdViewModel : OperatorBase<ImageThresholdInputs, ImageThresholdOutputs, GraphicsEmpty>
    {
        public ImageThresholdViewModel() : base()
        {
            Init();
            Inputs.PropertyChanged += Inputs_PropertyChanged;
        }

        [JsonConstructor]
        public ImageThresholdViewModel(ImageThresholdInputs inputs, ImageThresholdOutputs outputs, GraphicsEmpty graphics, RunStatus runStatus) 
            : base(inputs, outputs, graphics, runStatus)
        {
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
            Outputs.Image = Inputs.Image.Threshold(Inputs.ThresholdValue, Inputs.MaximumValue, Inputs.ThresholdType);
            //Cv2.Threshold(Inputs.Image, Outputs.Image, Inputs.ThresholdValue, Inputs.MaximumValue, Inputs.ThresholdType);

            Records[0].DisplayImage = Outputs.Image.ToBitmapSource();

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