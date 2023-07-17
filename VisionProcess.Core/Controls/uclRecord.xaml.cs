using System.Windows;
using System.Windows.Controls;
using VisionProcess.Core.ToolBase;

namespace VisionProcess.Core.Controls
{
    /// <summary>
    /// uclRecord.xaml 的交互逻辑
    /// </summary>
    public partial class uclRecord : UserControl
    {
        public uclRecord()
        {
            InitializeComponent();
        }

        public static readonly DependencyProperty RecordSourceProperty =
            DependencyProperty.Register(
                nameof(RecordSource),
                typeof(ICollection<Record>),
                typeof(uclRecord),
                new PropertyMetadata(null, RecordSourcePropertyChanged));

        public static readonly DependencyProperty SelectedRecordProperty =
            DependencyProperty.Register(
                nameof(SelectedRecord),
                typeof(Record),
                typeof(uclRecord),
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
            var uclRecord = (uclRecord)d;
            if (e.NewValue is not null)
            {
                uclRecord.SelectedRecord = ((ICollection<Record>)e.NewValue).FirstOrDefault();//默认选项
                //这里需要通知前台
            }
        }

        private static void SelectedRecordRecordChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            //uclRecord uclRecord = (uclRecord)d;
            //if (e.NewValue is not null)
            //{
            //    uclRecord.image.ImageSource = ((Record)e.NewValue).DisplayImage!;
            //}
            //else
            //{
            //    uclRecord.image.ImageSource = null;
            //}
        }
    }
}