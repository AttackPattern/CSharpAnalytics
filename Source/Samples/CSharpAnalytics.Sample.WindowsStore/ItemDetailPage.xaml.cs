using CSharpAnalytics.Sample.WindowsStore.Common;
using CSharpAnalytics.Sample.WindowsStore.Data;
using CSharpAnalytics.WindowsStore;
using System;
using System.Collections.Generic;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace CSharpAnalytics.Sample.WindowsStore
{
    /// <summary>
    /// A page that displays details for a single item within a group while allowing gestures to
    /// flip through other items belonging to the same group.
    /// </summary>
    public sealed partial class ItemDetailPage : ITrackPageView
    {
        private SampleDataItem item;

        public ItemDetailPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Populates the page with content passed during navigation.  Any saved state is also
        /// provided when recreating a page from a prior session.
        /// </summary>
        /// <param name="navigationParameter">The parameter value passed to
        /// <see cref="Frame.Navigate(Type, Object)"/> when this page was initially requested.
        /// </param>
        /// <param name="pageState">A dictionary of state preserved by this page during an earlier
        /// session.  This will be null the first time a page is visited.</param>
        protected override void LoadState(Object navigationParameter, Dictionary<String, Object> pageState)
        {
            // Allow saved page state to override the initial item to display
            if (pageState != null && pageState.ContainsKey("SelectedItem"))
            {
                navigationParameter = pageState["SelectedItem"];
            }

            // TODO: Create an appropriate data model for your problem domain to replace the sample data
            item = SampleDataSource.GetItem((String)navigationParameter);
            DefaultViewModel["Group"] = item.Group;
            DefaultViewModel["Items"] = item.Group.Items;
            flipView.SelectedItem = item;
            AutoAnalytics.Client.TrackAppView(item.Title);

            DataTransferManager.GetForCurrentView().DataRequested += OnDataRequested;
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);
            DataTransferManager.GetForCurrentView().DataRequested -= OnDataRequested;
        }

        private void OnDataRequested(DataTransferManager sender, DataRequestedEventArgs args)
        {
            var deferral = args.Request.GetDeferral();
            var data = args.Request.Data;
            data.SetText(item.Content);
            data.Properties.Title = item.Title;
            data.Properties.Description = item.Description;
            deferral.Complete();
        }

        /// <summary>
        /// Preserves state associated with this page in case the application is suspended or the
        /// page is discarded from the navigation cache.  Values must conform to the serialization
        /// requirements of <see cref="SuspensionManager.SessionState"/>.
        /// </summary>
        /// <param name="pageState">An empty dictionary to be populated with serializable state.</param>
        protected override void SaveState(Dictionary<String, Object> pageState)
        {
            var selectedItem = (SampleDataItem)flipView.SelectedItem;
            pageState["SelectedItem"] = selectedItem.UniqueId;
        }
    }
}