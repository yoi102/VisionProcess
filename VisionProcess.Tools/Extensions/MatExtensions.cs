using OpenCvSharp;
using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace VisionProcess.Tools.Extensions
{
    /// <summary>
    /// 提供 <see cref="Mat"/> 到 WPF <see cref="BitmapSource"/> 的线程安全转换。
    /// </summary>
    public static class MatExtensions
    {
        /// <summary>
        /// 将 <paramref name="mat"/> 转换为一个已冻结的 <see cref="BitmapSource"/>，
        /// 可在任意线程创建并安全地交给 UI 线程显示。
        /// </summary>
        /// <param name="mat">输入图像。</param>
        /// <returns>已冻结、不依赖创建线程的 <see cref="BitmapSource"/>。</returns>
        public static BitmapSource ToFrozenBitmapSource(this Mat mat)
        {
            if (mat is null)
            {
                throw new ArgumentNullException(nameof(mat));
            }

            // 通过 PNG 编码断开与原始 Mat 的关联，再解码为 WriteableBitmap 并 Freeze。
            // 这样得到的 BitmapSource 是 Freezable 且已冻结，没有线程亲和性，可直接跨线程使用。
            Cv2.ImEncode(".png", mat, out byte[] buffer);
            using var stream = new MemoryStream(buffer);

            var decoder = BitmapDecoder.Create(
                stream,
                BitmapCreateOptions.PreservePixelFormat,
                BitmapCacheOption.OnLoad);

            var writable = new WriteableBitmap(decoder.Frames[0]);
            writable.Freeze();
            return writable;
        }
    }
}
