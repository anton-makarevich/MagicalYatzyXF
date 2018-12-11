namespace Sanet.MagicalYatzy.Models.Common
{
    /// <summary>Size structure</summary>
    public struct Size
    {
        /// <summary>
        /// Width
        /// </summary>
        public double Width {get;}

        /// <summary>
        /// Height
        /// </summary>
        public double Height { get; }

        public Size(double width, double height)
        {
            Width = width;
            Height = height;
        }
    }
}
