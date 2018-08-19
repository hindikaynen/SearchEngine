using System.Diagnostics;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Navigation;

namespace DirectoryIndexerApp
{
    class HyperlinkBehavior
    {
        public static readonly DependencyProperty NavigateUriProperty = DependencyProperty.RegisterAttached(
            "NavigateUri", typeof(string), typeof(HyperlinkBehavior), new PropertyMetadata(default(string), OnNavigateUriChanged));

        public static void SetNavigateUri(DependencyObject element, string value)
        {
            element.SetValue(NavigateUriProperty, value);
        }

        public static string GetNavigateUri(DependencyObject element)
        {
            return (string) element.GetValue(NavigateUriProperty);
        }

        private static void OnNavigateUriChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Hyperlink hyperlink = (Hyperlink) d;
            hyperlink.Click -= OnClick;
            hyperlink.Click += OnClick;
        }

        private static void OnClick(object sender, RoutedEventArgs e)
        {
            Process.Start(GetNavigateUri((Hyperlink)sender));
        }
    }
}
