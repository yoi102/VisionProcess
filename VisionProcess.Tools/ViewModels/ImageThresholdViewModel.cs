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
                Records[^1].DisplayImage = Inputs.Image?.ToFrozenBitmapSource();
            }
        }

        protected override bool InternalExecute(out string message)
        {
            if (Inputs.Image is null)
            {
                message = "Input image can not be null";
                return false;
            }
            Outputs.Image ??= new Mat();
            Cv2.Threshold(Inputs.Image, Outputs.Image, Inputs.ThresholdValue, Inputs.MaximumValue, Inputs.ThresholdType);

            Records[0].DisplayImage = Outputs.Image.ToFrozenBitmapSource();

            message = Strings.Success;
            return true;
        }

        protected override IEnumerable<DefaultConnectorInfo> GetDefaultConnectors()
        {
            yield return new DefaultConnectorInfo(true, "Image", "Inputs.Image");
            yield return new DefaultConnectorInfo(true, "ThresholdValue", "Inputs.ThresholdValue");
            yield return new DefaultConnectorInfo(true, "MaximumValue", "Inputs.MaximumValue");
            yield return new DefaultConnectorInfo(false, "Image", "Outputs.Image");
        }

        private void Init()
        {
            Records.Add(new() { Title = Strings.OutputImage });
            Records.Add(new() { Title = Strings.InputImage });
        }
    }
}