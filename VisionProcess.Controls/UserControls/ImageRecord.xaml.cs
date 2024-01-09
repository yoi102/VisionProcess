using System.Windows;
using System.Windows.Controls;
using VisionProcess.Core.ToolBase;

namespace VisionProcess.Controls.UserControls
{
    /// <summary>
    /// ImageRecord.xaml 的交互逻辑
    /// </summary>
    public partial class ImageRecord : UserControl
    {
        public ImageRecord()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty RecordSourceProperty =
            DependencyProperty.Register(
                nameof(RecordSource),
                typeof(ICollection<Record>),
                typeof(ImageRecord),
                new PropertyMetadata(null, RecordSourcePropertyChanged));

        public static readonly DependencyProperty SelectedRecordProperty =
            DependencyProperty.Register(
                nameof(SelectedRecord),
                typeof(Record),
                typeof(ImageRecord),
                new PropertyMetadata(null, SelectedRecordRecordChanged));

        public ICollection<Record>? RecordSource
        {
            get { return (ICollection<Record>)GetValue(RecordSourceProperty); }
            set { SetValue(RecordSourceProperty, value); }
        }

        public Record? SelectedRecord
        {
            get { return (Record)GetValue(SelectedRecordProperty); }
            set { SetValue(SelectedRecordProperty, value); }
        }

        private static void RecordSourcePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var imageRecord = (ImageRecord)d;
            if (e.NewValue is not null)
            {
                imageRecord.SelectedRecord = ((ICollection<Record>)e.NewValue).FirstOrDefault();//默认选项
                //这里需要通知前台
            }
        }

        private static void SelectedRecordRecordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //ImageRecord ImageRecord = (ImageRecord)d;
            //if (e.NewValue is not null)
            //{
            //    ImageRecord.image.ImageSource = ((Record)e.NewValue).DisplayImage!;
            //}
            //else
            //{
            //    ImageRecord.image.ImageSource = null;
            //}
        }
    }
}