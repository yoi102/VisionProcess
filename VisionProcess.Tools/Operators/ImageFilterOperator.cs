using System;
using System.Collections.Generic;
using OpenCvSharp;
using VisionProcess.Abstractions.ToolBase;
using VisionProcess.Lang.Strings;
using VisionProcess.Tools.Models;

namespace VisionProcess.Tools.Operators
{
    /// <summary>
    /// 图像滤波算子。
    /// </summary>
    public sealed class ImageFilterOperator : OperatorBase<ImageFilterInputs, ImageFilterOutputs, GraphicsEmpty>
    {
        public ImageFilterOperator()
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
            switch (Inputs.FilterType)
            {
                case FilterTypes.NormalizedBox:
                    Cv2.Blur(Inputs.Image, Outputs.Image, new Size(Inputs.KernelWidth, Inputs.KernelHeight), borderType: Inputs.BorderType);
                    break;

                case FilterTypes.Median:
                    Cv2.MedianBlur(Inputs.Image, Outputs.Image, Inputs.KernelSize);
                    break;

                case FilterTypes.Gaussian:
                    Cv2.GaussianBlur(Inputs.Image, Outputs.Image, new Size(Inputs.KernelWidth, Inputs.KernelHeight), Inputs.SigmaX, Inputs.SigmaY, borderType: Inputs.BorderType);
                    break;

                case FilterTypes.Bilateral:
                    Cv2.BilateralFilter(Inputs.Image, Outputs.Image, Inputs.Diameter, Inputs.SigmaColor, Inputs.SigmaSpace, borderType: Inputs.BorderType);
                    break;

                default:
                    break;
            }

            Records[0].DisplayImage = Outputs.Image;
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
                Records[^1].DisplayImage = Inputs.Image;
            }
        }
    }
}
