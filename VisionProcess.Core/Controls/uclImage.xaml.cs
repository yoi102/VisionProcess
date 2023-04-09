using Microsoft.Win32;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Point = System.Windows.Point;

namespace VisionProcess.Core.Controls
{
    /// <summary>
    /// uclImage.xaml 的交互逻辑
    /// </summary>
    public partial class uclImage : UserControl
    {
        public uclImage()
        {
            InitializeComponent();
        }

        // Using a DependencyProperty as the backing store for ImageSource.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource",
                typeof(ImageSource), typeof(uclImage),
                new FrameworkPropertyMetadata(
                                null,
                                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                                new PropertyChangedCallback(OnImageSourceChanged),
                                null),
                        null);

        private Vec3b[,]? _ImageData3b;
        private byte[,]? _ImageDatab;
        private Point _MiddleButtonClickedPosition;
        private double _X;
        private double _Y;
        //记录中键点击的位置。。。。。鼠标中键拖拉移动用

        private int mouseDownCount = 0;

        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        private static void OnImageSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var uclImage = (uclImage)d;
            if (e.NewValue != null)
            {
                //uclImage!.ImageSource = (ImageSource)e.NewValue;
                uclImage.image.Source = (ImageSource)e.NewValue;
                uclImage.GetImageSourceData();
            }
            else
            {
                uclImage.image.Source = null;
                //uclImage!.ImageSource = null;
            }
        }

        private void BackFrame_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            //e.RightButton == MouseButtonState.Pressed)
            {
                mouseDownCount += 1;
                DispatcherTimer timer = new DispatcherTimer();
                timer.Interval = new TimeSpan(0, 0, 0, 0, 300);
                timer.Tick += (s, e1) => { timer.IsEnabled = false; mouseDownCount = 0; };
                timer.IsEnabled = true;
                if (mouseDownCount % 2 == 0)
                {
                    timer.IsEnabled = false;
                    mouseDownCount = 0;

                    var group = (TransformGroup)image.RenderTransform;
                    group.Children[0] = new ScaleTransform();
                    group.Children[1] = new TranslateTransform();
                }
            }
        }

        private void GetImageSourceData()
        {
            var image = ImageSource as BitmapSource;
            if (image != null)
            {
                using (Mat mat = image.ToMat())
                {
                    if (mat.Channels() == 3)
                    {
                        mat.GetRectangularArray<Vec3b>(out Vec3b[,] vec3Ds);
                        _ImageData3b = vec3Ds;
                        _ImageDatab = null;

                        GrayPanel.Visibility = Visibility.Collapsed;
                        RGBPanel.Visibility = Visibility.Visible;
                    }
                    else
                    {
                        mat.GetRectangularArray<byte>(out byte[,] vecDs);
                        _ImageDatab = vecDs;
                        _ImageData3b = null;
                        GrayPanel.Visibility = Visibility.Visible;
                        RGBPanel.Visibility = Visibility.Collapsed;
                    }
                }
            }
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            //e.RightButton == MouseButtonState.Pressed)
            {
                _MiddleButtonClickedPosition = e.GetPosition((IInputElement)e.Source);

                //mouseDownCount += 1;
                //DispatcherTimer timer = new DispatcherTimer();
                //timer.Interval = new TimeSpan(0, 0, 0, 0, 300);
                //timer.Tick += (s, e1) => { timer.IsEnabled = false; mouseDownCount = 0; };
                //timer.IsEnabled = true;
                //if (mouseDownCount % 2 == 0)
                //{
                //    timer.IsEnabled = false;
                //    mouseDownCount = 0;

                //    var group = (TransformGroup)image.RenderTransform;
                //    group.Children[0] = new ScaleTransform();
                //    group.Children[1] = new TranslateTransform();

                //}
            }
        }

        private void Image_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
        }

        private void Image_MouseMove(object sender, MouseEventArgs e)
        {
            var cursorPosition = e.GetPosition((IInputElement)e.Source);
            double x = cursorPosition.X;
            double y = cursorPosition.Y;
            //获取控件大小
            Image imageControl = (Image)(IInputElement)e.Source;

            double xRatio = ImageSource.Width / imageControl.ActualWidth;
            double yRatio = ImageSource.Height / imageControl.ActualHeight;

            _X = (float)(x * xRatio);
            _Y = (float)(y * yRatio);

            Path_X.Text = _X.ToString("0.00");
            Path_Y.Text = _Y.ToString("0.00");
            //获取图片像素信息
            if (_ImageData3b != null)
            {
                //准了
                Path_B.Text = _ImageData3b[(int)_Y, (int)_X].Item0.ToString("000");
                Path_G.Text = _ImageData3b[(int)_Y, (int)_X].Item1.ToString("000");
                Path_R.Text = _ImageData3b[(int)_Y, (int)_X].Item2.ToString("000");
            }
            else if (_ImageDatab != null)
            {
                Path_Gray.Text = _ImageDatab[(int)_Y, (int)_X].ToString("000");
                Path_Gray.Text = _ImageDatab[(int)_Y, (int)_X].ToString("000");
                Path_Gray.Text = _ImageDatab[(int)_Y, (int)_X].ToString("000");
            }

            //当中键按下，移动图片
            if (e.MiddleButton == MouseButtonState.Pressed)
            //e.RightButton == MouseButtonState.Pressed)
            {
                Image im = (Image)sender;
                var group = (TransformGroup)im.RenderTransform;
                var ttf = (TranslateTransform)group.Children[1];//对应Xaml位置    这样搞，放大缩小有点奇怪

                ttf.X += cursorPosition.X - _MiddleButtonClickedPosition.X;
                ttf.Y += cursorPosition.Y - _MiddleButtonClickedPosition.Y;
            }
        }

        //int mouseDownCount = 0;
        private void Image_MouseWheel(object sender, MouseWheelEventArgs e)
        {
            Image sf = (Image)sender;
            var group = (TransformGroup)sf.RenderTransform;
            var sc = (ScaleTransform)group.Children[0];//对应Xaml位置    这样搞，放大缩小有点奇怪
            var cursorPosition = e.GetPosition((IInputElement)e.Source);
            sc.CenterX = cursorPosition.X;
            sc.CenterY = cursorPosition.Y;
            //sc.ScaleX += e.Delta * 0.001;
            //sc.ScaleY += e.Delta * 0.001;

            if (e.Delta > 0)
            {
                sc.ScaleX += 0.05;
                sc.ScaleY += 0.05;
            }
            else
            {
                if (sc.ScaleX > 0.55)
                {
                    sc.ScaleX -= 0.05;
                    sc.ScaleY -= 0.05;
                }
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var scr = (ScrollViewer)ContextMenuService.GetPlacementTarget(LogicalTreeHelper.GetParent((DependencyObject)e.Source));

            var im = (Image)scr.Content;

            var group = (TransformGroup)im.RenderTransform;
            group.Children[0] = new ScaleTransform();
            group.Children[1] = new TranslateTransform();
        }

        private void MenuItem_Click_1(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog();
            // 设置对话框标题
            dialog.Title = Strings.Strings.SaveFile;
            // 设置默认文件名和文件类型
            dialog.FileName = Strings.Strings.SaveImage;
            dialog.DefaultExt = ".bmp";
            dialog.Filter = Strings.Strings.SaveImage + " (*.bmp)|*.bmp| (*.jpeg)|*.jpeg| (*.png)|*.png";

            // 显示对话框并获取用户选择的文件路径
            bool? result = dialog.ShowDialog();
            if (result == true)
            {
                string filePath = dialog.FileName;
                string fileExtension = Path.GetExtension(filePath);
                BitmapEncoder? encoder = fileExtension switch
                {
                    ".bmp" => new BmpBitmapEncoder(),
                    ".jpg" or ".jpeg" => new JpegBitmapEncoder(),
                    ".png" => new PngBitmapEncoder(),
                    _ => throw new NotSupportedException("Unsupported file format"),
                };
                if (encoder is not null)
                {
                    encoder.Frames.Add(BitmapFrame.Create((BitmapSource)ImageSource));
                    FileStream file = new FileStream(filePath, FileMode.Create);
                    encoder.Save(file);
                    file.Close();
                }
            }
        }
    }
}