using System;
using System.Windows.Forms;
using CSharpAnalytics.Sessions;

namespace CSharpAnalytics.Sample.WinForms
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private async void MainFormLoad(object sender, EventArgs e)
        {
            await AutoMeasurement.Start(new MeasurementConfiguration("UA-47064205-1", "My WinForms App", "1.0.0.0"));

            AllowUsageDataCollectionCheckBox.Checked = AutoMeasurement.VisitorStatus == VisitorStatus.Active;
        }

        private async void MainFormClosing(object sender, FormClosingEventArgs e)
        {
            await AutoMeasurement.End();
        }

        private void TrackScreenButtonClick(object sender, EventArgs e)
        {
            AutoMeasurement.Client.TrackAppView("My Shiny Screen");
        }

        private void TrackEventButtonClick(object sender, EventArgs e)
        {
            AutoMeasurement.Client.TrackEvent("My Action", "My Custom Category", "My Label");
        }

        private void AllowUsageDataCollectionCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            var isAlreadyActive = AutoMeasurement.VisitorStatus == VisitorStatus.Active;
            var value = AllowUsageDataCollectionCheckBox.Checked;

            if ((value && !isAlreadyActive) || (!value && isAlreadyActive))
            {
                AutoMeasurement.SetOptOut(!value);
            }
        }
    }
}