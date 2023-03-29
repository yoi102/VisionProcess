using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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




        public IEnumerable<Record> RecordSource
        {
            get { return (IEnumerable<Record>)GetValue(RecordsProperty); }
            set { SetValue(RecordsProperty, value); }
        }

        public static readonly DependencyProperty RecordsProperty =
            DependencyProperty.Register(
                nameof(RecordSource),
                typeof(IEnumerable<Record>), 
                typeof(uclRecord), 
                new PropertyMetadata(null, RecordsPropertyChanged));

        private static void RecordsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var uclRecord = (uclRecord)d;
            if (e.NewValue is not null)
            {
                uclRecord.SelectedRecord = ((IEnumerable<Record>)e.NewValue).FirstOrDefault()!;
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
