using VisionProcess.Abstractions.ToolBase;
using VisionProcess.Persistence.Operators;
using VisionProcess.Tools.Operators;

namespace VisionProcess.Persistence;

/// <summary>
/// Read-only compatibility boundary for v1 files that stored built-in operators as MessagePack unions.
/// New documents and new modules do not use this mapper.
/// </summary>
internal static class LegacyOperatorMapper
{
    public static IOperator? ToModel(OperatorDto dto)
        => dto switch
        {
            AcquireImageOperatorDto acquire => new AcquireImageOperator
            {
                CurrentIndex = acquire.CurrentIndex,
                ImagePaths = acquire.ImagePaths
            },
            ColorConvertOperatorDto color => new ColorConvertOperator
            {
                Inputs = { ColorConversionCodes = color.ColorConversionCodes }
            },
            ImageFilterOperatorDto filter => new ImageFilterOperator
            {
                Inputs =
                {
                    BorderType = filter.BorderType,
                    FilterType = filter.FilterType,
                    KernelHeight = filter.KernelHeight,
                    KernelWidth = filter.KernelWidth,
                    KernelSize = filter.KernelSize,
                    SigmaX = filter.SigmaX,
                    SigmaY = filter.SigmaY,
                    Diameter = filter.Diameter,
                    SigmaColor = filter.SigmaColor,
                    SigmaSpace = filter.SigmaSpace
                }
            },
            ImageThresholdOperatorDto threshold => new ImageThresholdOperator
            {
                Inputs =
                {
                    MaximumValue = threshold.MaximumValue,
                    ThresholdType = threshold.ThresholdType,
                    ThresholdValue = threshold.ThresholdValue
                }
            },
            _ => null
        };
}
