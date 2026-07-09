using Microsoft.Win32;
using System.Collections.Generic;
using VisionProcess.Abstractions.Services;
using VisionProcess.Lang.Strings;

namespace VisionProcess.wpf.Services
{
    /// <summary>
    /// 使用 WPF <see cref="OpenFileDialog"/> 实现文件选择服务。
    /// </summary>
    internal sealed class FileDialogService : IFileDialogService
    {
        public IReadOnlyList<string>? PickImageFiles()
        {
            var dialog = new OpenFileDialog
            {
                Multiselect = true,
                Filter = $"{Strings.ImageFiles}  (*.jpg*.bmp*.png)|*.jpg;*.bmp;*.png",
                Title = Strings.PleaseSelectFiles
            };

            bool? result = dialog.ShowDialog();
            if (result != true)
                return null;

            return dialog.FileNames;
        }
    }
}
