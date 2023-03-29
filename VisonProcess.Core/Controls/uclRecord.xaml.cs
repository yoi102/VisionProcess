using System;
using System.Collections;
using System.Collections.Generic;
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




        public IEnumerable Records
        {
            get { return (IEnumerable)GetValue(RecordsProperty); }
            set { SetValue(RecordsProperty, value); }
        }

        public static readonly DependencyProperty RecordsProperty =
            DependencyProperty.Register(nameof(Records), typeof(IEnumerable), typeof(uclRecord), new PropertyMetadata(null, RecordsPropertyChanged));

        private static void RecordsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {



        }







    }
}
