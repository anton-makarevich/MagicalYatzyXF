namespace Sanet.MagicalYatzy.Models.Common
{
    public struct Rectangle
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Height { get; set; }
        public double Width { get; set; }

        public double Left => X;
        public double Right => X + Width;
        public double Top => Y;
        public double Bottom => Y + Height;

        public Point TopLeft => new Point(Left, Top);
        public Point TopRight => new Point(Right, Top);
        public Point BottomLeft => new Point(Left, Bottom);
        public Point BottomRight => new Point(Right, Bottom);

        public Point Position => TopLeft;

        public Point[] Corners => new Point[] { TopLeft, TopRight, BottomRight, BottomLeft };

        public Rectangle(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;
        }

        public bool Contains(Point point)
        {
            if (point.X < Left ||
                point.X > Right ||
                point.Y < Top ||
                point.Y > Bottom)
                return false;
            return true;
        }

        public bool Intersects(Rectangle rectangle)
        {
            bool hasInside = false;
            bool hasOutside = false;
            foreach (var point in rectangle.Corners)
            {
                if (Contains(point))
                    hasInside = true;
                else
                    hasOutside = true;
            }
            return hasOutside && hasInside;
        }

        public bool Contains(Rectangle rectangle)
        {
            foreach (var point in rectangle.Corners)
            {
                if (!Contains(point))
                    return false;
            }
            return true;
        }
    }
}
