namespace Sanet.MagicalYatzy.Models.Common
{
    /// <summary>Point structure</summary>
    public struct Point
    {
        public double X {get; set; }

        public double Y { get; set; }

        public Point(double x, double y)
        {
            X = x;
            Y = y;
        }

        public bool IsZero
        {
            get
            {
                return X == 0 && Y == 0;
            }
        }

        public static Point operator+(Point p1, Point p2)
        {
            return new Point(p1.X + p2.X, p1.Y + p2.Y);
        }
    }
}
