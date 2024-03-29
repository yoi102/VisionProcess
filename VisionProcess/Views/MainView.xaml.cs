﻿using Microsoft.Extensions.DependencyInjection;
using System;
using System.Runtime.InteropServices;
using VisionProcess.ViewModels;

namespace VisionProcess.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView
    {
        public MainView()
        {
            InitializeComponent();

            this.DataContext = App.Current.Services.GetService<MainViewModel>();

            //虽然圆角，但影响发光边框。。。。。
            //https://learn.microsoft.com/en-us/windows/apps/desktop/modernize/apply-rounded-corners

            //IntPtr hWnd = new WindowInteropHelper(GetWindow(this)).EnsureHandle();
            //var attribute = DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE;
            //var preference = DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUND;   1
            //DwmSetWindowAttribute(hWnd, attribute, ref preference, sizeof(uint));

            //还有云母布局。。。
            //https://learn.microsoft.com/en-us/windows/apps/desktop/modernize/apply-snap-layout-menu

            //EfficiencyModeUtilities.SetEfficiencyMode(true);
            //WindowExtensions.Hide(this, enableEfficiencyMode: true); // default value
            //WindowExtensions.Show(this, disableEfficiencyMode: true);// default value
            //ic.ForceCreate(enablesEfficiencyMode: true); // default value
        }

        // The enum flag for DwmSetWindowAttribute's second parameter, which tells the function what attribute to set.
        // Copied from dwmapi.h
        public enum DWMWINDOWATTRIBUTE
        {
            DWMWA_WINDOW_CORNER_PREFERENCE = 33
        }

        // The DWM_WINDOW_CORNER_PREFERENCE enum for DwmSetWindowAttribute's third parameter, which tells the function
        // what value of the enum to set.
        // Copied from dwmapi.h
        public enum DWM_WINDOW_CORNER_PREFERENCE
        {
            DWMWCP_DEFAULT = 0,
            DWMWCP_DONOTROUND = 1,
            DWMWCP_ROUND = 2,
            DWMWCP_ROUNDSMALL = 3
        }

        // Import dwmapi.dll and define DwmSetWindowAttribute in C# corresponding to the native function.
        [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
        internal static extern void DwmSetWindowAttribute(IntPtr hwnd,
                                                         DWMWINDOWATTRIBUTE attribute,
                                                         ref DWM_WINDOW_CORNER_PREFERENCE pvAttribute,
                                                         uint cbAttribute);
    }
}