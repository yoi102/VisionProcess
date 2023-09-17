using MahApps.Metro.Controls;
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

        public static readonly DependencyProperty ImageSourceProperty =
            DependencyProperty.Register("ImageSource",
                typeof(ImageSource), typeof(uclImage),
                new FrameworkPropertyMetadata(
                                null,
                                FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsRender,
                                new PropertyChangedCallback(OnImageSourceChanged),
                                null),
                        null);

        public static readonly DependencyProperty TitleProperty =
    DependencyProperty.Register("Title", typeof(string), typeof(uclImage), new PropertyMetadata(""));

        private byte[,]? imageData;

        private Vec3b[,]? imageData3b;

        private int maxX;

        private int maxY;

        private Point middleButtonClickedPosition;

        private int mouseDownCount = 0;

        public uclImage()
        {
            InitializeComponent();
        }
        public ImageSource ImageSource
        {
            get { return (ImageSource)GetValue(ImageSourceProperty); }
            set { SetValue(ImageSourceProperty, value); }
        }

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
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
                DispatcherTimer timer = new();
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
            if (ImageSource is BitmapSource image)
            {
                using Mat mat = image.ToMat();
                if (mat.Channels() == 3)
                {
                    mat.GetRectangularArray(out Vec3b[,] vec3Ds);
                    imageData3b = vec3Ds;
                    maxY = imageData3b.GetLength(0);
                    maxX = imageData3b.GetLength(1);
                    imageData = null;
                    GrayPanel.Visibility = Visibility.Collapsed;
                    RGBPanel.Visibility = Visibility.Visible;
                }
                else
                {
                    mat.GetRectangularArray(out byte[,] vecDs);
                    imageData = vecDs;
                    imageData3b = null;
                    maxY = 0;
                    maxX = 0;
                    GrayPanel.Visibility = Visibility.Visible;
                    RGBPanel.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void Image_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            //e.RightButton == MouseButtonState.Pressed)
            {
                middleButtonClickedPosition = e.GetPosition((IInputElement)e.Source);

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

            //获取控件大小
            Image imageControl = (Image)(IInputElement)e.Source;

            double xRatio = ImageSource.Width / imageControl.ActualWidth;
            double yRatio = ImageSource.Height / imageControl.ActualHeight;

            double x = cursorPosition.X * xRatio;
            double y = cursorPosition.Y * yRatio;
            int int_x = (int)x;
            int int_y = (int)y;

            Path_X.Text = x.ToString("0.00");
            Path_Y.Text = y.ToString("0.00");
            //获取图片像素信息
            if (imageData3b != null)
            {
                if (y < maxY && x < maxX)
                {
                    //准了
                    Path_B.Text = imageData3b[int_y, int_x].Item0.ToString("000");
                    Path_G.Text = imageData3b[int_y, int_x].Item1.ToString("000");
                    Path_R.Text = imageData3b[int_y, int_x].Item2.ToString("000");
                }
            }
            else if (imageData != null)
            {
                Path_Gray.Text = imageData[int_y, int_x].ToString("000");
                Path_Gray.Text = imageData[int_y, int_x].ToString("000");
                Path_Gray.Text = imageData[int_y, int_x].ToString("000");
            }

            //当中键按下，移动图片
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                Image im = (Image)sender;
                var group = (TransformGroup)im.RenderTransform;
                var ttf = (TranslateTransform)group.Children[1];//对应Xaml位置    这样搞，放大缩小有点奇怪

                ttf.X += cursorPosition.X - middleButtonClickedPosition.X;
                ttf.Y += cursorPosition.Y - middleButtonClickedPosition.Y;
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
                    FileStream file = new(filePath, FileMode.Create);
                    encoder.Save(file);
                    file.Close();
                }
            }
        }

        private void OpenInNewWindow(object sender, RoutedEventArgs e)
        {
            var window = new MetroWindow();
            window.Width = 800;
            window.Height = 600;
            window.Title = Title;
            window.ResizeMode = ResizeMode.CanResizeWithGrip;
            window.TitleCharacterCasing = CharacterCasing.Normal;
            uclImage imageEx = new uclImage
            {
                ImageSource = ImageSource,
                Title = Title
            };
            imageEx.OpenInNewWindowMenuItem.Visibility = Visibility.Collapsed;
            window.Content = imageEx;
            window.ShowDialog();
            //window.Show();
        }
    }
}