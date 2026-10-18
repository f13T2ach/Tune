using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Tune.Components.Content
{
    /// <summary>
    /// Shows one panel at a time, with a triangle on each side to turn the pages. The left
    /// triangle carries the number of the page being shown and the right one carries how many
    /// there are, which is the same pair the header writes out.
    /// </summary>
    [ContentProperty(nameof(Pages))]
    public partial class PanelSwitcher : UserControl
    {
        /// <summary>The name of a page, written in the header next to the position.</summary>
        public static readonly DependencyProperty PageTitleProperty =
            DependencyProperty.RegisterAttached(
                "PageTitle",
                typeof(string),
                typeof(PanelSwitcher),
                new PropertyMetadata(string.Empty));

        private int _currentIndex;

        public PanelSwitcher()
        {
            InitializeComponent();

            Pages.CollectionChanged += (_, _) => ShowCurrent();
        }

        /// <summary>Gets the panels, in the order they are shown.</summary>
        public ObservableCollection<object> Pages { get; } = new ObservableCollection<object>();

        /// <summary>Gets or sets which page is shown, counted from zero.</summary>
        public int CurrentIndex
        {
            get => _currentIndex;
            set
            {
                int wanted = Math.Clamp(value, 0, Math.Max(0, Pages.Count - 1));
                if (wanted == _currentIndex) return;

                _currentIndex = wanted;
                ShowCurrent();
            }
        }

        /// <summary>Gets or sets the name of a page, which the header shows.</summary>
        public static string GetPageTitle(DependencyObject element) =>
            (string)element.GetValue(PageTitleProperty);

        /// <summary>Sets the name of a page, which the header shows.</summary>
        public static void SetPageTitle(DependencyObject element, string value) =>
            element.SetValue(PageTitleProperty, value);

        private void Previous_Click(object sender, RoutedEventArgs e) => CurrentIndex--;

        private void Next_Click(object sender, RoutedEventArgs e) => CurrentIndex++;

        /// <summary>
        /// Puts the current page in place and brings the header and both triangles up to date,
        /// including which way may still be turned.
        /// </summary>
        private void ShowCurrent()
        {
            PART_Presenter.Content = Pages.Count > 0 ? Pages[_currentIndex] : null;

            PART_Previous.IsEnabled = _currentIndex > 0;
            PART_Next.IsEnabled = _currentIndex < Pages.Count - 1;

            PART_CurrentNumber.Text = Pages.Count > 0 ? (_currentIndex + 1).ToString() : string.Empty;
            PART_TotalNumber.Text = Pages.Count > 0 ? Pages.Count.ToString() : string.Empty;

string title = string.Empty;

            if (Pages.Count > 0 && Pages[_currentIndex] is DependencyObject page)
                title = GetPageTitle(page);
            PART_Header.Text = Pages.Count > 0
                ? $"{title} {_currentIndex + 1}/{Pages.Count}".Trim()
                : string.Empty;
        }
    }
}
