using System;
using System.Collections.Generic;
using OpenCvSharp;
using VisionProcess.Abstractions.ToolBase;
using VisionProcess.Lang.Strings;
using VisionProcess.Tools.Models;

namespace VisionProcess.Tools.Operators
{
    /// <summary>
    /// 图像阈值算子。
    /// </summary>
    public sealed class ImageThresholdOperator : OperatorBase<ImageThresholdInputs, ImageThresholdOutputs, GraphicsEmpty>
    {
        public ImageThresholdOperator()
        {
            Init();
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
            Cv2.Threshold(Inputs.Image, Outputs.Image, Inputs.ThresholdValue, Inputs.MaximumValue, Inputs.ThresholdType);

            Records[0].DisplayImage = Outputs.Image;

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

        private void Inputs_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName?.Equals(nameof(Inputs.Image)) == true)
            {
                Records[^1].DisplayImage = Inputs.Image;
            }
        }
    }
}
