using OpenCvSharp;
using System;
using System.IO;
using System.Windows.Media.Imaging;

namespace VisionProcess.wpf.Extensions
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
        /// <remarks>
        /// OpenCV 的 PNG 编码器只接受 1/3/4 通道且深度为 8U/16U 的图像。
        /// 遇到不符合的 Mat 时，本方法会先把它转换为 8UC3（BGR）再编码，避免运行时抛异常。
        /// </remarks>
        /// <param name="mat">输入图像。</param>
        /// <returns>已冻结、不依赖创建线程的 <see cref="BitmapSource"/>。</returns>
        public static BitmapSource ToFrozenBitmapSource(this Mat mat)
        {
            if (mat is null)
            {
                throw new ArgumentNullException(nameof(mat));
            }

            // 先归一化为 PNG 编码器支持的格式。
            using var normalized = EnsurePngCompatible(mat);

            // 通过 PNG 编码断开与原始 Mat 的关联，再解码为 WriteableBitmap 并 Freeze。
            // 这样得到的 BitmapSource 是 Freezable 且已冻结，没有线程亲和性，可直接跨线程使用。
            Cv2.ImEncode(".png", normalized, out byte[] buffer);
            using var stream = new MemoryStream(buffer);

            var decoder = BitmapDecoder.Create(
                stream,
                BitmapCreateOptions.PreservePixelFormat,
                BitmapCacheOption.OnLoad);

            var writable = new WriteableBitmap(decoder.Frames[0]);
            writable.Freeze();
            return writable;
        }

        /// <summary>
        /// 将 <paramref name="mat"/> 转换为 PNG 编码器可接受的 1/3/4 通道、8U/16U 格式。
        /// 非标准通道数会先被归一到 3 通道 BGR；非 8U/16U 深度会被归一到 8U。
        /// </summary>
        private static Mat EnsurePngCompatible(Mat mat)
        {
            int channels = mat.Channels();
            bool channelOk = channels is 1 or 3 or 4;
            bool depthOk = mat.Depth() is MatType.CV_8U or MatType.CV_16U;

            if (channelOk && depthOk)
            {
                return mat.Clone();
            }

            // 先把深度归一到 8U，同时保留原通道数。
            using var depthAdjusted = new Mat();
            if (mat.Depth() != MatType.CV_8U)
            {
                var targetType = (MatType)(MatType.CV_8U + ((channels - 1) << 3));
                mat.ConvertTo(depthAdjusted, targetType);
            }
            else
            {
                mat.CopyTo(depthAdjusted);
            }

            var result = new Mat();
            int adjustedChannels = depthAdjusted.Channels();

            if (adjustedChannels == 1)
            {
                Cv2.CvtColor(depthAdjusted, result, ColorConversionCodes.GRAY2BGR);
            }
            else if (adjustedChannels == 2)
            {
                var split = Cv2.Split(depthAdjusted);
                using var zero = new Mat(depthAdjusted.Rows, depthAdjusted.Cols, MatType.CV_8UC1, Scalar.Black);
                Cv2.Merge(new[] { split[0], split[1], zero }, result);
                DisposeAll(split);
            }
            else // >= 3
            {
                var split = Cv2.Split(depthAdjusted);
                Cv2.Merge(new[] { split[0], split[1], split[2] }, result);
                DisposeAll(split);
            }

            return result;
        }

        private static void DisposeAll(Mat[] mats)
        {
            foreach (var m in mats)
            {
                m.Dispose();
            }
        }
    }
}
