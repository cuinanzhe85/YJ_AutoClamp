using Microsoft.Xaml.Behaviors;
using System;
using System.Windows.Controls;
using System.Windows.Threading;

namespace YJ_AutoClamp.Utils
{
    public class ListBoxScrollIntoViewBehavior : Behavior<ListBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.SelectionChanged += AssociatedObject_SelectionChanged;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.SelectionChanged -= AssociatedObject_SelectionChanged;
            base.OnDetaching();
        }

        private static void AssociatedObject_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var listBox = sender as ListBox;

            if (listBox?.SelectedItem == null)
                return;

            Action action = () =>
            {
                listBox.UpdateLayout();

                // 선택된 아이템으로 스크롤 이동시키기.
                if (listBox.SelectedItem != null)
                    listBox.ScrollIntoView(listBox.SelectedItem);
            };

            listBox.Dispatcher.BeginInvoke(action, DispatcherPriority.ContextIdle);
        }
    }

}
