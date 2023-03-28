using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace VisonProcess.Core.ToolBase
{
    public class Record : ObservableObject
    {


        private string? _tiltle;
        public string? Tiltie
        {
            get { return _tiltle; }
            set { SetProperty(ref _tiltle, value); }
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
                    OnPropertyChanged();
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
