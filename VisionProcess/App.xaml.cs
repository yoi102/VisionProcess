using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using VisionProcess.Core.Helpers;
using VisionProcess.Core.ToolBase;
using VisionProcess.ViewModels;

namespace VisionProcess
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private static Mutex? appMutex;

        //查找，启动可能会慢
        public static readonly IEnumerable<Type> ToolViewModelTypes = GetToolViewModels(ReflectionHelper.GetAllReferencedAssemblies());

        public static IEnumerable<Type> GetToolViewModels(IEnumerable<Assembly> assemblies)
        {
            List<Type> viewModels = new List<Type>();
            foreach (var asm in assemblies)
            {
                var types = asm.GetTypes().Where(t => t.IsAbstract == false && t.IsAssignableTo(typeof(IOperation)));
                viewModels.AddRange(types);
            }
            return viewModels;
        }

        /// <summary>
        /// Gets the current <see cref="App"/> instance in use
        /// </summary>
        public new static App Current => (App)Application.Current;

        /// <summary>
        /// Gets the <see cref="IServiceProvider"/> instance to resolve application services.
        /// </summary>
        public IServiceProvider Services { get; }

        public App()
        {
            string lang = System.Globalization.CultureInfo.CurrentCulture.Name;
            //lang = "ja-jp";
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

        protected override void OnExit(ExitEventArgs e)
        {
            var mainViewModel = Services.GetService<MainViewModel>()
                                ?? throw new ArgumentNullException(nameof(MainViewModel));

            if (File.Exists(@"configs\mainViewModel.config"))
            {
                // serialize JSON to a string and then write string to a file
                JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    TypeNameHandling = TypeNameHandling.Auto,
                    Formatting = Formatting.Indented,
                    DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
                    DateParseHandling = DateParseHandling.DateTime
                };
                File.WriteAllText(@"configs\mainViewModel.config", JsonConvert.SerializeObject(mainViewModel, jsonSerializerSettings));
            }
            else
            {
                // serialize JSON directly to a file
                using (StreamWriter file = File.CreateText(@"configs\mainViewModel.config"))
                {
                    JsonSerializer serializer = new()
                    {
                        NullValueHandling = NullValueHandling.Ignore,
                        TypeNameHandling = TypeNameHandling.Auto,
                        Formatting = Formatting.Indented,
                        DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
                        DateParseHandling = DateParseHandling.DateTime
                    };
                    serializer.Serialize(file, mainViewModel);
                }
            }
            base.OnExit(e);
        }

        /// <summary>
        /// Configures the services for the application.
        /// </summary>
        private static IServiceProvider ConfigureServices()
        {
            if (!Directory.Exists(@"configs"))   //如果文件夹不存在则创建
            {
                Directory.CreateDirectory(@"configs");
            }

            var services = new ServiceCollection();
            if (File.Exists(@"configs\mainViewModel.config"))
            {
                // serialize JSON to a string and then write string to a file
                var mainViewModel = JsonConvert.DeserializeObject<MainViewModel>(File.ReadAllText(@"configs\mainViewModel.config"), new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore,
                    TypeNameHandling = TypeNameHandling.Auto,
                    Formatting = Formatting.Indented,
                    DateFormatHandling = DateFormatHandling.MicrosoftDateFormat,
                    DateParseHandling = DateParseHandling.DateTime
                });
                if (mainViewModel is null)
                    throw new ArgumentNullException();
                services.AddSingleton(o => mainViewModel);
            }
            else
            {
                services.AddSingleton<MainViewModel>();
            }
            services.AddTransient<EditorViewModel>();

            foreach (var itemType in ToolViewModelTypes)//遍历所有类型进行查找
            {
                services.AddTransient(itemType);
                //list.Add(itemType.Name.Replace("ViewModel", string.Empty));
            }

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

        [DllImport("User32.dll")]
        private static extern int ShowWindow(IntPtr hwnd, uint nCmdShow);

        [DllImport("User32.DLL")]
        public static extern bool SetForegroundWindow(IntPtr hWnd);

        //[DllImport("User32.dll")]
        //[return: MarshalAs(UnmanagedType.Bool)]
        //public static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [DllImport("User32.dll", CharSet = CharSet.Unicode, EntryPoint = "FlashWindow")]
        public static extern void FlashWindow(IntPtr hwnd, bool bInvert);
    }
}