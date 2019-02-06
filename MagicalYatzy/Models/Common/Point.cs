namespace Sanet.MagicalYatzy.Models.Common
{
    public struct Point
    {
        public double X {get; }

        public double Y { get; }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        public bool IsZero => X == 0 && Y == 0;

        public static Point operator+(Point p1, Point p2)
        {
            return new Point(p1.X + p2.X, p1.Y + p2.Y);
        }
    }
}
