using Newtonsoft.Json;
using OpenCvSharp.WpfExtensions;
using System.Collections.ObjectModel;
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
    public class ImageThresholdViewModel : OperationBase<ImageThresholdInputs, ImageThresholdOutputs, GraphicsEmpty>
    {
        public ImageThresholdViewModel() : base()
        {
            Init();
            Inputs.PropertyChanged += Inputs_PropertyChanged;
        }

        [JsonConstructor]
        public ImageThresholdViewModel(string name, GraphicsEmpty graphic, ImageThresholdInputs inputs,
                 ImageThresholdOutputs outputs, bool isRealTime, ObservableCollection<Record> records, RunStatus runStatus)
                 : base(name,graphic, inputs,
                 outputs, isRealTime, records, runStatus)
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