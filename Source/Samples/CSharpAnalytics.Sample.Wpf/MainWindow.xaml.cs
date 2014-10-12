using System.Diagnostics;
using System.Windows;
using CSharpAnalytics.Sessions;

namespace CSharpAnalytics.Sample.Wpf
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            AutoMeasurement.DebugWriter = d => Debug.WriteLine(d);
            AutoMeasurement.Start(new MeasurementConfiguration("UA-319000-8"));

            AllowUsageDataCollectionCheckBox.IsChecked = AutoMeasurement.VisitorStatus == VisitorStatus.Active;
        }

        private void TrackScreenOnClick(object sender, RoutedEventArgs e)
        {
            AutoMeasurement.Client.TrackScreenView("My Shiny Screen");
        }

        private void TrackEventOnClick(object sender, RoutedEventArgs e)
        {
            AutoMeasurement.Client.TrackEvent("My Action", "My Custom Category", "My Label");
        }
    }
}