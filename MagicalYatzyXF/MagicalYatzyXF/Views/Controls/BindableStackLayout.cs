using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;
using Xamarin.Forms;

namespace Sanet.MagicalYatzy.XF.Views.Controls
{
    public class BindableStackLayout: StackLayout
    {
        public static readonly BindableProperty ItemTemplateSelectorProperty = 
            BindableProperty.Create(
                nameof(ItemTemplate),
                typeof(DataTemplate),
                typeof(BindableStackLayout), 
                default(DataTemplate),
                BindingMode.TwoWay);

        public DataTemplate ItemTemplate
        {
            get => (DataTemplate)GetValue(ItemTemplateSelectorProperty);
            set => SetValue(ItemTemplateSelectorProperty, value);
        }
        
        public static readonly BindableProperty ItemsSourceProperty = 
            BindableProperty.Create(
                nameof(ItemsSource), 
                typeof(IEnumerable), 
                typeof(HorizontalListView),
                null, BindingMode.TwoWay, 
                null, OnItemsSourceChanged);

        public IEnumerable ItemsSource
        {
            get => (IEnumerable)GetValue(ItemsSourceProperty);
            set => SetValue(ItemsSourceProperty, value);
        }
        
        private static void OnItemsSourceChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (oldValue != null && oldValue.Equals(newValue))
                return;
            var stack = bindable as BindableStackLayout;
            stack?.UpdateItems();

            stack?.ObservableUpdateItems(stack, (IEnumerable)newValue);
        }
        
        private void ObservableUpdateItems(BindableStackLayout stack, IEnumerable newValue)
        {
            if (newValue is INotifyCollectionChanged observableCollection)
            {
                observableCollection.CollectionChanged += (sender, args) =>
                {
                    switch (args.Action)
                    {
                        case NotifyCollectionChangedAction.Reset:
                            Children.Clear();
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
                            foreach (var obj in items)
                            {
                                var item = Children.FirstOrDefault(x => ReferenceEquals(x.BindingContext, obj));
                                if (item != null)
                                    Children.Remove(item);
                            }
                            break;
                        case NotifyCollectionChangedAction.Replace:
                            break;
                    }
                };
            }
        }
        
        private void AddItem(IEnumerable newItems)
        {
            foreach (var newItem in newItems)
            {
                CreateItem(newItem);
            }
        }
        
        private void CreateItem(object item)
        {
            var oldItem = Children.FirstOrDefault(x => ReferenceEquals(x.BindingContext, item));
            if (oldItem != null)
                return;
            
            var content = ItemTemplate.CreateContent();

            var viewCell = (ViewCell)content;
            if (viewCell == null) return;
            viewCell.BindingContext = item;
            viewCell.View.BindingContext = item;

            Children.Add(viewCell.View);
        }

        public void UpdateItems()
        {
            try
            {
                Children.Clear();
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
    }
}