using System;
using Sanet.MagicalYatzy.Services.Media;

namespace Sanet.MagicalYatzy.Avalonia.Services.Stubs;

public class SoundsProviderStub:ISoundsProvider
{
    public void PlaySound(string sound)
    {
        Console.WriteLine($"Playing sound: {sound}");
    }
}