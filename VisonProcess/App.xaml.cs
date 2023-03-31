using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using VisonProcess.ViewModels;
using static ControlzEx.Standard.NativeMethods;

namespace VisonProcess
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex? appMutex;


        public App()
        {

            string lang = System.Globalization.CultureInfo.CurrentCulture.Name;
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo(lang); ;
            Thread.CurrentThread.CurrentUICulture = new System.Globalization.CultureInfo(lang); ;

            Services = ConfigureServices();
            this.InitializeComponent();


        }

        protected override void OnStartup(StartupEventArgs e)
        {



            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            CheckMutex(e);
        }







        /// <summary>
        /// Gets the current <see cref="App"/> instance in use
        /// </summary>
        public new static App Current => (App)Application.Current;

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
        /// </summary>
        public IServiceProvider Services { get; }

        /// <summary>
        /// Configures the services for the application.
        /// </summary>
        private static IServiceProvider ConfigureServices()
        {
            var services = new ServiceCollection();

            services.AddSingleton<MainViewModel>();
            //services.AddSingleton<IFilesService, FilesService>();
            //services.AddSingleton<ISettingsService, SettingsService>();
            //services.AddSingleton<IClipboardService, ClipboardService>();
            //services.AddSingleton<IShareService, ShareService>();
            //services.AddSingleton<IEmailService, EmailService>();

            return services.BuildServiceProvider();
        }


















        private void CheckMutex(StartupEventArgs e)
        {
            Process currentProc = Process.GetCurrentProcess();
            appMutex = new System.Threading.Mutex(true, currentProc.ProcessName, out bool createdNew);
            if (createdNew)
            {
                base.OnStartup(e);
            }
            else
            {
                foreach (Process proc in Process.GetProcessesByName(currentProc.ProcessName))
                {
                    if (proc.Id != currentProc.Id)
                    {
                        //这里不作用。PID是正确的
                        //IntPtr handle = proc.Handle;
                        //这里也能起作用，也是用的是主窗口名称
                        IntPtr handle = proc.MainWindowHandle;
                        //var ffi = Create_FLASHWINFO(handle, FlashWindowFlag.FLASHW_TIMERNOFG, 500, 5000);
                        //FlashWindowEx(ref ffi);
                        ShowWindow(handle, 9);
                        SetForegroundWindow(handle);
                        break;
                    }
                }
                //var hwnd = FindWindow(null, currentProc.ProcessName);//找string的窗口
                ////var fi = User32Api.Create_FLASHWINFO(hwnd, FlashWindowFlag.FLASHW_TIMERNOFG, 1, 2000);
                ////FlashWindowEx(ref fi);
                //// FlashWindow(hwnd, true);//Flash 会有点慢
                //ShowWindow(hwnd, 9);
                //SetForegroundWindow(hwnd);//使用的窗口名称
                //Process.GetCurrentProcess().Kill();
                //App.Current.Shutdown();
                Environment.Exit(0);
            }
        }

        [DllImport("User32.dll", EntryPoint = "FindWindow")]
        public static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll")]
        private static extern int ShowWindow(IntPtr hwnd, uint nCmdShow);

        [DllImport("USER32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [DllImport("User32.dll", CharSet = CharSet.Unicode, EntryPoint = "FlashWindow")]
        public static extern void FlashWindow(IntPtr hwnd, bool bInvert);





    }
}
