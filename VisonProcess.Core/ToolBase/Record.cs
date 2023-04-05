using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows.Media.Imaging;

namespace VisonProcess.Core.ToolBase
{
    public class Record : ObservableObject
    {
        private string? _title;

        public string? Title
        {
            get { return _title; }
            set { SetProperty(ref _title, value); }
        }

        private BitmapSource? _displayImage;

        public BitmapSource? DisplayImage
        {
            get { return _displayImage; }
            set
            {
                if (value != _displayImage)
                {
                    _displayImage = value;
                    _displayImage?.Freeze();
                    //只能这样更新？
                    System.Windows.Application.Current.Dispatcher.Invoke(() =>
                    {
                        OnPropertyChanged();
                    });
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