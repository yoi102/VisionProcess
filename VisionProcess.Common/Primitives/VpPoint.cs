using System;

namespace VisionProcess.Common.Primitives
{
    /// <summary>
    /// 二维点，替代 <see cref="System.Windows.Point"/>，使 Runtime 等层不再依赖 WPF。
    /// </summary>
    public readonly struct VpPoint : IEquatable<VpPoint>
    {
        public double X { get; }

        public double Y { get; }

        public VpPoint()
        {
            X = 0;
            Y = 0;
        }

        public VpPoint(double x, double y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object? obj) => obj is VpPoint other && Equals(other);

        public bool Equals(VpPoint other) => X == other.X && Y == other.Y;

        public override int GetHashCode() => HashCode.Combine(X, Y);

        public override string ToString() => $"({X}, {Y})";

        public static bool operator ==(VpPoint left, VpPoint right) => left.Equals(right);

        public static bool operator !=(VpPoint left, VpPoint right) => !left.Equals(right);
    }
}
