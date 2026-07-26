using OpenCvSharp;
using VisionProcess.Abstractions.ToolBase;

namespace VisionProcess.Tools.Models;

/// <summary>
/// Common image result and metadata exposed by image-processing operators.
/// </summary>
public abstract class ImageOutputsBase : OutputsBase
{
    private Mat? image;

    public Mat? Image
    {
        get => image;
        internal set
        {
            if (ReferenceEquals(image, value))
                return;

            image?.Dispose();
            image = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Width));
            OnPropertyChanged(nameof(Height));
            OnPropertyChanged(nameof(Channels));
            OnPropertyChanged(nameof(PixelFormat));
            OnPropertyChanged(nameof(PixelCount));
            OnPropertyChanged(nameof(HasImage));
        }
    }

    public int Width => image?.Width ?? 0;

    public int Height => image?.Height ?? 0;

    public int Channels => image?.Channels() ?? 0;

    public string PixelFormat => image?.Type().ToString() ?? string.Empty;

    public long PixelCount => image?.Total() ?? 0;

    public bool HasImage => image is not null && !image.Empty();

    protected sealed override void ConfigureConnectors(ConnectorProviderBuilder builder)
    {
        builder.Add<Mat?>("Image", "Image", () => Image, null);
        builder.Add<int>("Width", "Width", () => Width, null);
        builder.Add<int>("Height", "Height", () => Height, null);
        builder.Add<int>("Channels", "Channels", () => Channels, null);
        builder.Add<string>("PixelFormat", "PixelFormat", () => PixelFormat, null);
        builder.Add<long>("PixelCount", "PixelCount", () => PixelCount, null);
        builder.Add<bool>("HasImage", "HasImage", () => HasImage, null);
        ConfigureAdditionalConnectors(builder);
    }

    protected virtual void ConfigureAdditionalConnectors(ConnectorProviderBuilder builder)
    {
    }
}
