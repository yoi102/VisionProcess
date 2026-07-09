using System;

namespace VisionProcess.Common.Primitives
{
    /// <summary>
    /// 二维尺寸，替代 <see cref="System.Windows.Size"/>，使 Runtime 等层不再依赖 WPF。
    /// </summary>
    public readonly struct VpSize : IEquatable<VpSize>
    {
        public double Width { get; }

        public double Height { get; }

        public VpSize()
        {
            Width = 0;
            Height = 0;
        }

        public VpSize(double width, double height)
        {
            Width = width;
            Height = height;
        }

        public override bool Equals(object? obj) => obj is VpSize other && Equals(other);

        public bool Equals(VpSize other) => Width == other.Width && Height == other.Height;

        public override int GetHashCode() => HashCode.Combine(Width, Height);

        public override string ToString() => $"{Width} x {Height}";

        public static bool operator ==(VpSize left, VpSize right) => left.Equals(right);

        public static bool operator !=(VpSize left, VpSize right) => !left.Equals(right);
    }
}
