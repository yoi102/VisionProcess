using System;
using System.Collections.Generic;
using OpenCvSharp;
using VisionProcess.Abstractions.ToolBase;
using VisionProcess.Lang.Strings;
using VisionProcess.Tools.Models;

namespace VisionProcess.Tools.Operators
{
    /// <summary>
    /// 颜色转换算子。
    /// </summary>
    public sealed class ColorConvertOperator : OperatorBase<ColorConvertInputs, ColorConvertOutputs, GraphicsEmpty>
    {
        public ColorConvertOperator()
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
            var output = new Mat();
            Cv2.CvtColor(Inputs.Image, output, ToOpenCvCode(Inputs.ColorConversionCodes));
            Outputs.Image = output;
            Records[0].DisplayImage = Outputs.Image;

            message = Strings.Success;
            return true;
        }

        protected override IEnumerable<DefaultConnectorInfo> GetDefaultConnectors()
        {
            yield return new DefaultConnectorInfo(true, "Image", "Inputs.Image");
            yield return new DefaultConnectorInfo(false, "Image", "Outputs.Image");
        }

        private static ColorConversionCodes ToOpenCvCode(ColorConvertType type)
        {
            return type switch
            {
                ColorConvertType.BGR2GRAY => ColorConversionCodes.BGR2GRAY,
                ColorConvertType.BGR2RGB => ColorConversionCodes.BGR2RGB,
                ColorConvertType.BGR2HSV => ColorConversionCodes.BGR2HSV,
                ColorConvertType.BGR2HLS => ColorConversionCodes.BGR2HLS,
                ColorConvertType.BGR2YUV => ColorConversionCodes.BGR2YUV,
                ColorConvertType.YUV2BGR => ColorConversionCodes.YUV2BGR,
                ColorConvertType.BGR2Lab => ColorConversionCodes.BGR2Lab,
                ColorConvertType.Lab2BGR => ColorConversionCodes.Lab2BGR,
                ColorConvertType.RGB2GRAY => ColorConversionCodes.RGB2GRAY,
                ColorConvertType.RGB2BGR => ColorConversionCodes.RGB2BGR,
                ColorConvertType.GRAY2BGR => ColorConversionCodes.GRAY2BGR,
                _ => ColorConversionCodes.BGR2GRAY,
            };
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
