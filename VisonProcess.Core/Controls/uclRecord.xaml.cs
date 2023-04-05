using System.Windows;
using System.Windows.Controls;
using VisonProcess.Core.ToolBase;

namespace VisonProcess.Core.Controls
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

        public ICollection<Record> RecordSource
        {
            get { return (ICollection<Record>)GetValue(RecordsProperty); }
            set { SetValue(RecordsProperty, value); }
        }

        public static readonly DependencyProperty RecordsProperty =
            DependencyProperty.Register(
                nameof(RecordSource),
                typeof(ICollection<Record>),
                typeof(uclRecord),
                new PropertyMetadata(null, RecordsPropertyChanged));

        private static void RecordsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var uclRecord = (uclRecord)d;
            if (e.NewValue is not null)
            {
                uclRecord.SelectedRecord = ((ICollection<Record>)e.NewValue).FirstOrDefault()!;

                //这里需要通知前台
            }
        }

        public Record SelectedRecord
        {
            get { return (Record)GetValue(SelectedRecordProperty); }
            set { SetValue(SelectedRecordProperty, value); }
        }

        public static readonly DependencyProperty SelectedRecordProperty =
            DependencyProperty.Register(
                nameof(SelectedRecord),
                typeof(Record),
                typeof(uclRecord),
                new PropertyMetadata(null, SelectedRecordRecordChenged));

        private static void SelectedRecordRecordChenged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
        }
    }
}