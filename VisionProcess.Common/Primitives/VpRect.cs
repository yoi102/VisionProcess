using System;

namespace VisionProcess.Common.Primitives
{
    /// <summary>
    /// 二维矩形，替代 <see cref="System.Windows.Rect"/>，使 Runtime 等层不再依赖 WPF。
    /// </summary>
    public readonly struct VpRect : IEquatable<VpRect>
    {
        public double X { get; }

        public double Y { get; }

        public double Width { get; }

        public double Height { get; }

        public VpPoint Location => new(X, Y);

        public VpSize Size => new(Width, Height);

        public VpRect()
        {
            X = 0;
            Y = 0;
            Width = 0;
            Height = 0;
        }

        public VpRect(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public VpRect(VpPoint location, VpSize size)
        {
            X = location.X;
            Y = location.Y;
            Width = size.Width;
            Height = size.Height;
        }

        public override bool Equals(object? obj) => obj is VpRect other && Equals(other);

        public bool Equals(VpRect other) =>
            X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;

        public override int GetHashCode() => HashCode.Combine(X, Y, Width, Height);

        public override string ToString() => $"{X},{Y},{Width},{Height}";

        public static bool operator ==(VpRect left, VpRect right) => left.Equals(right);

        public static bool operator !=(VpRect left, VpRect right) => !left.Equals(right);
    }
}
