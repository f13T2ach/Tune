using System.Globalization;
using System.Windows;
using WPFLocalizeExtension.Engine;

namespace Tune
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            LocalizeDictionary.Instance.Culture = new CultureInfo("zh-CN");
            //this.WindowState = WindowState.Maximized;
        }


    }
}