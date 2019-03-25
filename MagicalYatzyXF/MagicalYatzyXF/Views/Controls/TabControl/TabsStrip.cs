using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Xamarin.Forms;

namespace Sanet.MagicalYatzy.XF.Views.Controls.TabControl
{
    public class TabsStrip: ContentView
    {
        #region Private Members

        private bool _inTransition;

        private readonly ObservableCollection<TabItem> _children = new ObservableCollection<TabItem>();

        private readonly ContentView _tabControl;

        private readonly Grid _contentView;

        private readonly StackLayout _buttonStack;

        private readonly TabBarIndicator _indicator;

        #endregion

        #region Events

        public event EventHandler<int> TabActivated;

        #endregion

        public TabsStrip()
        {
            var mainLayout = new RelativeLayout();
            Content = mainLayout;

            // Create tab control
            _buttonStack = new StackLayoutExtended
            {
                Orientation = StackOrientation.Horizontal,
                Padding = 0,
                Spacing = 0,
                HorizontalOptions = LayoutOptions.FillAndExpand,
                VerticalOptions = LayoutOptions.FillAndExpand,
            };

            _indicator = new TabBarIndicator
            {
                VerticalOptions = LayoutOptions.End,
                HorizontalOptions = LayoutOptions.Start,
                BackgroundColor = (Color)TabIndicatorColorProperty.DefaultValue,
                HeightRequest = 6,
                WidthRequest = 0,
            };

            _tabControl = new ContentView
            {
                BackgroundColor = TabBackColor,
                Content = new Grid
                {
                    Padding = 0,
                    ColumnSpacing = 0,
                    RowSpacing = 0,
                    Children = {
                        _buttonStack,
                        _indicator,
                    }
                }
            };

            mainLayout.Children.Add(_tabControl, () => new Rectangle(
               0, 0, mainLayout.Width, TabHeight)
            );

            // Create content control
            _contentView = new Grid
            {
                ColumnSpacing = 0,
                RowSpacing = 0,
                Padding = 0,
                BackgroundColor = Color.Transparent,
            };

            mainLayout.Children.Add(_contentView, () => new Rectangle(
               0, TabHeight, mainLayout.Width, mainLayout.Height - TabHeight)
            );

            _children.CollectionChanged += (sender, e) => {
                foreach (var view in _buttonStack.Children)
                {
                    var tabButton = (TabBarButton) view;
                    tabButton.ButtonPressed -= TabButtonPressed;
                }

                _contentView.Children.Clear();
                _buttonStack.Children.Clear();

                foreach (var tabChild in TabChildren)
                {
                    var tabItemControl = new TabBarButton(tabChild.Title);
                    if (FontFamily != null)
                        tabItemControl.FontFamily = FontFamily;

                    tabItemControl.FontSize = FontSize;
                    tabItemControl.SelectedColor = TabIndicatorColor;
                    _buttonStack.Children.Add(tabItemControl);
                    tabItemControl.ButtonPressed += TabButtonPressed;
                }

                if (TabChildren.Any())
                    Activate(TabChildren.First(), false);
            };
        }

        private void TabButtonPressed(object sender, EventArgs e)
        {
            if (sender is TabBarButton button)
            {
                var idx = _buttonStack.Children.IndexOf(button);
                Activate(TabChildren[idx], true);
            }
        }

        public void Activate(TabItem tabChild, bool animate)
        {
            if (tabChild == null)
                return;
            var existingChild = TabChildren.FirstOrDefault(t => t.View ==
               _contentView.Children.FirstOrDefault(v => v.IsVisible));

            if (existingChild == tabChild)
                return;

            var idxOfExisting = existingChild != null ? TabChildren.IndexOf(existingChild) : -1;
            var idxOfNew = TabChildren.IndexOf(tabChild);

            if (idxOfExisting > -1 && animate)
            {
                _inTransition = true;

                // Animate
                var translation = idxOfExisting < idxOfNew ?
                    _contentView.Width : -_contentView.Width;

                tabChild.View.TranslationX = translation;
                if (tabChild.View.Parent != _contentView)
                    _contentView.Children.Add(tabChild.View);
                else
                    tabChild.View.IsVisible = true;

                var newElementWidth = _buttonStack.Children.ElementAt(idxOfNew).Width;
                var newElementLeft = _buttonStack.Children.ElementAt(idxOfNew).X;

                var animation = new Animation();
                var existingViewOutAnimation = new Animation((d) =>
                    {
                        if (existingChild != null) existingChild.View.TranslationX = d;
                    },
                    0, -translation, Easing.CubicInOut, () =>
                    {
                        if (existingChild != null) existingChild.View.IsVisible = false;
                        _inTransition = false;
                    });

                var newViewInAnimation = new Animation((d) => tabChild.View.TranslationX = d,
                    translation, 0, Easing.CubicInOut);

                var existingTranslation = _indicator.TranslationX;

                var indicatorTranslation = newElementLeft;
                var indicatorViewAnimation = new Animation((d) => _indicator.TranslationX = d,
                    existingTranslation, indicatorTranslation, Easing.CubicInOut);

                var startWidth = _indicator.Width;
                var indicatorSizeAnimation = new Animation((d) => _indicator.WidthRequest = d,
                    startWidth, newElementWidth, Easing.CubicInOut);

                animation.Add(0.0, 1.0, existingViewOutAnimation);
                animation.Add(0.0, 1.0, newViewInAnimation);
                animation.Add(0.0, 1.0, indicatorViewAnimation);
                animation.Add(0.0, 1.0, indicatorSizeAnimation);
                animation.Commit(this, "TabAnimation");
            }
            else
            {
                // Just set first view
                _contentView.Children.Clear();
                _contentView.Children.Add(tabChild.View);
            }

            foreach (var tabBtn in _buttonStack.Children)
                ((TabBarButton)tabBtn).IsSelected = _buttonStack.Children.IndexOf(tabBtn) == idxOfNew;

            TabActivated?.Invoke(this, idxOfNew);
        }

        protected override void LayoutChildren(double x, double y, double width, double height)
        {
            base.LayoutChildren(x, y, width, height);

            if (width > 0 && !_inTransition)
            {
                var existingChild = TabChildren.FirstOrDefault(t =>
                   t.View == _contentView.Children.FirstOrDefault(v => v.IsVisible));

                var idxOfExisting = existingChild != null ? TabChildren.IndexOf(existingChild) : -1;

                _indicator.WidthRequest = _buttonStack.Children.ElementAt(idxOfExisting).Width;
                _indicator.TranslationX = _buttonStack.Children.ElementAt(idxOfExisting).X;
            }
        }

        public IList<TabItem> TabChildren => _children;

        public static BindableProperty FontSizeProperty =
            BindableProperty.Create(nameof(FontSize), typeof(double), typeof(TabsStrip), 14.0,
                propertyChanged: (bindable, oldValue, newValue) => {
                    var ctrl = (TabsStrip)bindable;
                    ctrl.FontSize = (double)newValue;
                });

        public double FontSize
        {
            get { return (double)GetValue(FontSizeProperty); }
            set
            {
                SetValue(FontSizeProperty, value);
                foreach (var tabBtn in _buttonStack.Children)
                    ((TabBarButton)tabBtn).FontSize = value;
            }
        }

        public static BindableProperty FontFamilyProperty =
            BindableProperty.Create(nameof(FontFamily), typeof(string), typeof(TabsStrip), null,
                propertyChanged: (bindable, oldValue, newValue) => {
                    var ctrl = (TabsStrip)bindable;
                    ctrl.FontFamily = (string)newValue;
                });

        public string FontFamily
        {
            get { return (string)GetValue(FontFamilyProperty); }
            set
            {
                SetValue(FontFamilyProperty, value);
                foreach (var tabBtn in _buttonStack.Children)
                    ((TabBarButton)tabBtn).FontFamily = value;
            }
        }

        public static BindableProperty TabIndicatorColorProperty =
            BindableProperty.Create(nameof(TabIndicatorColor), typeof(Color), typeof(TabsStrip), Color.Accent,
                propertyChanged: (bindable, oldValue, newValue) => {
                    var ctrl = (TabsStrip)bindable;
                    ctrl.TabIndicatorColor = (Color)newValue;
                });

        public Color TabIndicatorColor
        {
            get { return (Color)GetValue(TabIndicatorColorProperty); }
            set
            {
                SetValue(TabIndicatorColorProperty, value);
                _indicator.BackgroundColor = value;
            }
        }

       public static BindableProperty TabHeightProperty =
            BindableProperty.Create(nameof(TabHeight), typeof(double), typeof(TabsStrip), 40.0,
                propertyChanged: (bindable, oldValue, newValue) => {
                    var ctrl = (TabsStrip)bindable;
                    ctrl.TabHeight = (double)newValue;
                });

        public double TabHeight
        {
            get { return (double)GetValue(TabHeightProperty); }
            set
            {
                SetValue(TabHeightProperty, value);
            }
        }

       public static BindableProperty TabBackColorProperty =
            BindableProperty.Create(nameof(TabBackColor), typeof(Color), typeof(TabsStrip), Color.White,
                propertyChanged: (bindable, oldValue, newValue) => {
                    var ctrl = (TabsStrip)bindable;
                    ctrl.TabBackColor = (Color)newValue;
                });

        public Color TabBackColor
        {
            get { return (Color)GetValue(TabBackColorProperty); }
            set
            {
                SetValue(TabBackColorProperty, value);
                _tabControl.BackgroundColor = value;
            }
        }
    }
}
