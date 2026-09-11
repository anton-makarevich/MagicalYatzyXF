using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using Sanet.MagicalYatzy.Xf.Extensions;
using Xamarin.Forms;

namespace Sanet.MagicalYatzy.Xf.Views.Controls
{
    public class HorizontalListView : ScrollView
    {
        private StackLayout _stack;
        private const double CellSize = 50.0;

        public event EventHandler<SelectEventArgs> ItemSelected;

        public HorizontalListView()
        {
            Init();
        }

        private void Init()
        {
            _stack = new StackLayout
            {
                Spacing = 0,
                Orientation = StackOrientation.Horizontal,
            };
            Orientation = ScrollOrientation.Horizontal;
            Content = _stack;
        }

        public static readonly BindableProperty ItemTemplateSelectorProperty = BindableProperty.Create(nameof(ItemTemplate), typeof(DataTemplate), typeof(HorizontalListView), default(DataTemplate), BindingMode.TwoWay, null, null, null, null);

        public DataTemplate ItemTemplate
        {
            get
            {
                return (DataTemplate)GetValue(ItemTemplateSelectorProperty);
            }
            set
            {
                SetValue(ItemTemplateSelectorProperty, value);
            }
        }

        public static readonly BindableProperty SelectColorProperty = BindableProperty.Create(nameof(SelectedItemColor), typeof(Color), typeof(HorizontalListView), Color.Red, BindingMode.TwoWay, null, null, null, null);
        
        public Color SelectedItemColor
        {
            get { return (Color)GetValue(SelectColorProperty); }
            set { SetValue(SelectColorProperty, value); }
        }

        public static readonly BindableProperty SelectedItemProperty = BindableProperty.Create(nameof(SelectedItem), typeof(object), typeof(HorizontalListView), null, BindingMode.TwoWay, null, OnSelectedItemChanged, null, null);
        
        private static void OnSelectedItemChanged(BindableObject bindable, object oldValue, object newValue)
        {
            var stack = bindable as HorizontalListView;
            stack?.SelectObject(newValue);
        }

        public static readonly BindableProperty ItemsSourceProperty = BindableProperty.Create(nameof(ItemsSource), typeof(IEnumerable), typeof(HorizontalListView), null, BindingMode.TwoWay, null, OnItemsSourceChanged, null, null);

        private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (oldValue != null && oldValue.Equals(newValue))
                return;
            var stack = bindable as HorizontalListView;
            stack?.UpdateItems();

            stack?.ObservableUpdateItems(stack, (IEnumerable)newValue);
        }

        private void ObservableUpdateItems(HorizontalListView stack, IEnumerable newValue)
        {
            if (newValue is INotifyCollectionChanged observableCollection)
            {
                observableCollection.CollectionChanged += (sender, args) =>
                {
                    switch (args.Action)
                    {
                        case NotifyCollectionChangedAction.Reset:
                            stack.ClearItems();
                            break;
                        case NotifyCollectionChangedAction.Add:
                            stack.AddItem(args.NewItems);
                            break;
                        case NotifyCollectionChangedAction.Move:
                            break;
                        case NotifyCollectionChangedAction.Remove:
                            var eventArgs = args;
                            var items = eventArgs.OldItems;
                            if (items == null) return;
                            foreach (object o in items)
                            {
                                var item = _stack.Children.FirstOrDefault(x => ReferenceEquals(x.BindingContext, o));
                                _stack.Children.Remove(item);
                            }
                            break;
                        case NotifyCollectionChangedAction.Replace:
                            break;
                    }
                };
            }
        }

        public Object SelectedItem
        {
            get { return (Object)GetValue(SelectedItemProperty); }
            set { SetValue(SelectedItemProperty, value); }
        }

        public void UpdateItems()
        {
            try
            {
                _stack.Children.Clear();
                if (!IsVisible)
                    return;
                if (ItemsSource == null)
                {
                    return;
                }
                foreach (var item in ItemsSource)
                {
                    CreateItem(item);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private void CreateItem(object item)
        {
            var content = ItemTemplate.CreateContent();

            var viewCell = (ViewCell)content;
            if (viewCell != null)
            {
                viewCell.BindingContext = item;
                viewCell.View.BindingContext = item;
                _stack.Children.Add(viewCell.View);

                var tap = new TapGestureRecognizer();
                tap.Tapped += (s, e) =>
                {
                    var view = (View)s;
                    view.AnimateClick();
                    SelectItem(item, view);
                };
                viewCell.View.GestureRecognizers.Add(tap);
            }
        }

        private void AddItem(IList newItems)
        {
            foreach (var newItem in newItems)
            {
                CreateItem(newItem);
            }
        }

        private void SelectObject(object item)
        {
            View view = null;
            foreach (var itemView in _stack.Children)
            {
                if (((View)itemView).BindingContext == item)
                {
                    view = (View)itemView;
                    break;
                }
            }

            SelectItem(item, view);
        }

        private void SelectItem(object selectItem, View selectView)
        {
            foreach (var itemView in _stack.Children)
            {
                itemView.BackgroundColor = Color.Transparent;
            }

            if (selectView != null)
            {
                ItemSelected?.Invoke(this, new SelectEventArgs(selectItem, selectView));

                if ((selectView.X + selectView.Width - ScrollX) > Width)
                {
                    double delta = selectView.X + selectView.Width - ScrollX - Width;
                    ScrollToAsync(ScrollX + delta, ScrollY, true);
                }
                else if (selectView.X - ScrollX < 0)
                {
                    double delta = ScrollX - selectView.X;
                    ScrollToAsync(ScrollX - delta, ScrollY, true);
                }
                selectView.BackgroundColor = SelectedItemColor;
            }
        }

        private void ClearItems()
        {
            _stack.Children.Clear();
        }

        public IEnumerable ItemsSource
        {
            get { return (IEnumerable)GetValue(ItemsSourceProperty); }
            set
            {
                SetValue(ItemsSourceProperty, value);
            }
        }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);
            if (propertyName == nameof(IsVisible) && IsVisible && !_stack.Children.Any())
                UpdateItems();
        }
    }

    public class SelectEventArgs : EventArgs
    {
        public readonly object ItemData;
        public readonly object ItemView;

        public SelectEventArgs(object itemData, object itemView)
        {
            ItemData = itemData;
            ItemView = itemView;
        }
    }
}
