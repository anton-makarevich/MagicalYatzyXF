using Sanet.MagicalYatzy.Models;
using System;

namespace Sanet.MagicalYatzy.Services
{
    public interface IExternalNavigationService
    {
        void OpenYatzyFBPage();
        void RateApp();
        void SendFeedback();
    }
}