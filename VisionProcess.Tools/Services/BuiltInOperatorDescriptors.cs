using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using OpenCvSharp;
using VisionProcess.Abstractions.ToolBase;
using VisionProcess.Tools.Models;
using VisionProcess.Tools.Operators;

namespace VisionProcess.Tools.Services;

/// <summary>
/// Registration boundary for the operators shipped with VisionProcess.
/// External module packages can contribute additional descriptors at composition time.
/// </summary>
public static class BuiltInOperatorDescriptors
{
    public static IReadOnlyList<OperatorDescriptor> All { get; } =
    [
        Create<AcquireImageOperator, AcquireImageState>(
            "AcquireImage",
            static op => new(op.CurrentIndex, op.ImagePaths?.ToList() ?? []),
            static state => new AcquireImageOperator
            {
                CurrentIndex = state.CurrentIndex,
                ImagePaths = state.ImagePaths
            }),
        Create<ImageFilterOperator, ImageFilterState>(
            "ImageFilter",
            static op => new(
                op.Inputs.BorderType,
                op.Inputs.FilterType,
                op.Inputs.KernelHeight,
                op.Inputs.KernelWidth,
                op.Inputs.KernelSize,
                op.Inputs.SigmaX,
                op.Inputs.SigmaY,
                op.Inputs.Diameter,
                op.Inputs.SigmaColor,
                op.Inputs.SigmaSpace),
            static state => new ImageFilterOperator
            {
                Inputs =
                {
                    BorderType = state.BorderType,
                    FilterType = state.FilterType,
                    KernelHeight = state.KernelHeight,
                    KernelWidth = state.KernelWidth,
                    KernelSize = state.KernelSize,
                    SigmaX = state.SigmaX,
                    SigmaY = state.SigmaY,
                    Diameter = state.Diameter,
                    SigmaColor = state.SigmaColor,
                    SigmaSpace = state.SigmaSpace
                }
            }),
        Create<ImageThresholdOperator, ImageThresholdState>(
            "ImageThreshold",
            static op => new(
                op.Inputs.MaximumValue,
                op.Inputs.ThresholdType,
                op.Inputs.ThresholdValue),
            static state => new ImageThresholdOperator
            {
                Inputs =
                {
                    MaximumValue = state.MaximumValue,
                    ThresholdType = state.ThresholdType,
                    ThresholdValue = state.ThresholdValue
                }
            }),
        Create<ColorConvertOperator, ColorConvertState>(
            "ColorConvert",
            static op => new(op.Inputs.ColorConversionCodes),
            static state => new ColorConvertOperator
            {
                Inputs = { ColorConversionCodes = state.ColorConversionCodes }
            })
    ];

    private static OperatorDescriptor Create<TOperator, TState>(
        string id,
        Func<TOperator, TState> captureState,
        Func<TState, TOperator> restoreState)
        where TOperator : class, IOperator, new()
        => new(
            id,
            id,
            typeof(TOperator),
            static () => new TOperator(),
            stateVersion: 1,
            @operator => JsonSerializer.SerializeToUtf8Bytes(
                captureState((TOperator)@operator)),
            (version, payload) =>
            {
                if (version != 1)
                {
                    throw new NotSupportedException(
                        $"Operator '{id}' state version {version} is not supported.");
                }

                var state = JsonSerializer.Deserialize<TState>(payload)
                    ?? throw new InvalidOperationException(
                        $"Operator '{id}' state payload is empty.");
                return restoreState(state);
            });

    private sealed record AcquireImageState(int CurrentIndex, List<string> ImagePaths);

    private sealed record ColorConvertState(ColorConvertType ColorConversionCodes);

    private sealed record ImageThresholdState(
        double MaximumValue,
        ThresholdTypes ThresholdType,
        double ThresholdValue);

    private sealed record ImageFilterState(
        BorderTypes BorderType,
        FilterTypes FilterType,
        int KernelHeight,
        int KernelWidth,
        int KernelSize,
        double SigmaX,
        double SigmaY,
        int Diameter,
        double SigmaColor,
        double SigmaSpace);
}
