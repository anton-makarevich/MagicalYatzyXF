namespace Sanet.MagicalYatzy.XF.Extensions
{
    public static class ModelConverters
    {
        public static Xamarin.Forms.Rectangle ToFormsRectangle(this Sanet.MagicalYatzy.Models.Common.Rectangle rectangle)
        {
            return new Xamarin.Forms.Rectangle(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
        }
    }
}
