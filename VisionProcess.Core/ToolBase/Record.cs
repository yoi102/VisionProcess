using CommunityToolkit.Mvvm.ComponentModel;
using Newtonsoft.Json;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.Windows.Media.Imaging;

namespace VisionProcess.Core.ToolBase
{
    public class Record : ObservableObject
    {
        private string? title;

        public string? Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }

        private BitmapSource? displayImage;

        [JsonIgnore]
        public BitmapSource? DisplayImage
        {
            get
            {
                return displayImage;
            }
            set
            {
                if (value != displayImage)
                {
                    displayImage = value;
                    displayImage?.Freeze();
                    //System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    //{
                    OnPropertyChanged();
                    //});
                }
            }
        }

        [JsonProperty("DisplayImage")]
        public byte[]? DisplayImageBytes
        {
            get
            {
                if (displayImage is null)
                    return null;
                return BitmapSourceConverter.ToMat(displayImage).ToBytes();
            }
            set
            {
                if (value is not null)
                {
                    using var mat = Mat.FromArray(value);
                    DisplayImage = mat.ToBitmapSource();
                }
            }
        }

        //private string _shapes;
        //public string Shapes
        //{
        //    get { return _shapes; }
        //    set { SetProperty(ref _shapes, value); }
        //}
    }
}