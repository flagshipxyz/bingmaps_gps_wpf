using System.Windows;
using BingMaps_GPS_WPF.ViewModel;
using FirstFloor.ModernUI.Windows.Controls;

namespace BingMaps_GPS_WPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : ModernWindow
    {
        /// <summary>
        /// Initializes a new instance of the MainWindow class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            Closed += (s, e) => ViewModelLocator.Cleanup();
            //Closing += (s, e) => ViewModelLocator.Cleanup();
        }
    }
}