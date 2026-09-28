using Sanet.MVVM.Core.ViewModels;

namespace Sanet.MagicalYatzy.ViewModels;

public sealed class OnlinePlayerViewModel : BindableBase
{
    public OnlinePlayerViewModel(string name, bool isReady, bool isHost, bool isLocal)
    {
        Name = name;
        IsReady = isReady;
        IsHost = isHost;
        IsLocal = isLocal;
    }

    public string Name { get; }

    public bool IsReady { get; }

    public bool IsHost { get; }

    public bool IsLocal { get; }
}