using Microsoft.Xaml.Behaviors;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace WpfSample
{
    public class ScrollToEndBehavior : Behavior<ItemsControl>
    {
        private ScrollViewer? _scrollViewer;
        private bool _autoScroll = true;

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += OnLoaded;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.Loaded -= OnLoaded;

            if (AssociatedObject.ItemsSource is INotifyCollectionChanged observable)
            {
                observable.CollectionChanged -= OnCollectionChanged;
            }

            if (_scrollViewer != null)
            {
                _scrollViewer.ScrollChanged -= OnScrollChanged;
            }
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            _scrollViewer = FindScrollViewer(AssociatedObject);

            if (_scrollViewer != null)
            {
                _scrollViewer.ScrollChanged += OnScrollChanged;
            }

            ScrollToEnd();

            if (AssociatedObject.ItemsSource is INotifyCollectionChanged observable)
            {
                observable.CollectionChanged += OnCollectionChanged;
            }
        }

        private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (_scrollViewer == null) return;

            bool isAtBottom = _scrollViewer.VerticalOffset >= _scrollViewer.ScrollableHeight - 5;

            if (!isAtBottom && _autoScroll)
            {
                _autoScroll = false;
            }
            else if (isAtBottom && !_autoScroll)
            {
                _autoScroll = true;
            }
        }

        private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add && _autoScroll)
            {
                ScrollToEnd();
            }
        }

        private void ScrollToEnd()
        {
            _scrollViewer?.ScrollToEnd();
        }

        private static ScrollViewer? FindScrollViewer(DependencyObject parent)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is ScrollViewer scrollViewer)
                    return scrollViewer;

                var result = FindScrollViewer(child);
                if (result != null)
                    return result;
            }

            return null;
        }
    }
}