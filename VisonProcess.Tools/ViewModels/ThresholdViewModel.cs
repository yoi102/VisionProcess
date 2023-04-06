using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VisonProcess.Core.Attributes;
using VisonProcess.Core.Strings;
using VisonProcess.Core.ToolBase;
using VisonProcess.Tools.Models;

namespace VisonProcess.Tools.ViewModels
{
    [DefaultToolConnector(true, "Image", "Inputs.Image")]
    [DefaultToolConnector(true, "Threshold", "Inputs.Threshold")]
    [DefaultToolConnector(true, "MaxValue", "Inputs.MaxValue")]
    [DefaultToolConnector(false, "Image", "Outputs.Image")]
    public class ThresholdViewModel : OperationBase<ThresholdInputs, ThresholdOutputs, GraphicsBase>
    {
        public ThresholdViewModel() : base()
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
            Outputs.Image ??= new Mat();
            Cv2.Threshold(Inputs.Image, Outputs.Image, Inputs.Threshold, Inputs.MaxValue, Inputs.ThresholdType);
            Records[0].DisplayImage = Outputs.Image.ToBitmapSource();


            return true;

        }

        private void Init()
        {
            Records.Add(new() { Title = Strings.OutputImage });
        }




    }
}
