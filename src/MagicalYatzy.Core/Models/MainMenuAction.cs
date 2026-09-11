using System.Windows.Input;

namespace Sanet.MagicalYatzy.Models;

public record MainMenuAction(string Label, string Description, string Image, ICommand MenuAction);