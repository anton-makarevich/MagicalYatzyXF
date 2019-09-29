using Sanet.MagicalYatzy.Services.Navigation;

#if __IOS__
namespace Sanet.MagicalYatzy.Xf.Ios.Services
#elif __MACOS__
namespace Sanet.MagicalYatzy.Xf.Mac.Services
#endif
{
    public class ExternalNavigationService : IExternalNavigationService
    {
        public void OpenYatzyFBPage()
        {   
        }

        public void RateApp()
        { 
        }

        public void SendFeedback()
        {  
        }
    }
}