﻿using OpenCvSharp.WpfExtensions;
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

        }

        protected override bool InternalExecute(out string message)
        {
            message = "";
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
        }
    }
}