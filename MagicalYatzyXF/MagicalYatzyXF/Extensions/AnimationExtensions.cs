using Xamarin.Forms;

namespace Sanet.MagicalYatzy.XF.Extensions
{
    public static class AnimationExtensions
    {
        static bool _isRuning;
        public async static void AnimateClick(this VisualElement element)
        {
            if (_isRuning)
                return;
            _isRuning = true;
            await element.ScaleTo(0.95);
            await element.ScaleTo(1);
            _isRuning = false;
        }
    }
}
