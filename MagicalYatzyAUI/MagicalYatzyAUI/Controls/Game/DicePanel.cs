// using Avalonia.Controls;
// using Sanet.MagicalYatzy.Avalonia.Controls.Interactions;
// using Sanet.MagicalYatzy.Models.Game;
// using Sanet.MagicalYatzy.Views;
//
// namespace Sanet.MagicalYatzy.Avalonia.Controls.Game
// {
//     public class DicePanel: Canvas, IDicePanelView
//     {
//         private IDicePanel _dicePanelModel;
//         private readonly DicePanelTapRecognizer _tapRecognizer;
//
//         public DicePanel(bool addTabRecognizer = false)
//         {
//             if (!addTabRecognizer) return;
//             _tapRecognizer = new DicePanelTapRecognizer();
//             _tapRecognizer.Tapped += (sender, point) =>
//             {
//                 _dicePanelModel.DieClicked(point.ToSanetPoint());
//             };
//             Children.Add(_tapRecognizer);
//         }
//
//         public IDicePanel DicePanel
//         {
//             get => _dicePanelModel;
//             set
//             {
//                 // TODO clear handlers
//                 _dicePanelModel = value;
//                 
//                 // TODO add handlers
//                 _dicePanelModel.DieAdded += OnDieAdded;
//             }
//         }
//
//         private void OnDieAdded(object sender, Die die)
//         {
//             Children.Insert(0, new DieImage(die));
//         }
//
//         protected override void LayoutChildren(double x, double y, double width, double height)
//         {
//             if (_tapRecognizer != null)
//             {
//                 _tapRecognizer.WidthRequest = width;
//                 _tapRecognizer.HeightRequest = height;
//             }
//             
//             base.LayoutChildren(x, y, width, height);
//             if (width > 0 && height > 0)
//                 _dicePanelModel.Resize((int)width, (int)height);
//         }
//     }
// }
