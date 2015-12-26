using System;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using App1.Models;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace App1.Pages
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DetailsPage : Page
    {
        private OmnivaLocation _details;

        public DetailsPage()
        {
            InitializeComponent();
        }

        private void btnBack_Click(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }

        private async void btnNavigateTo_Click(object sender, RoutedEventArgs e)
        {
            Uri driveToUri = new Uri($"ms-drive-to:?destination.latitude={_details.YCoordinate}&destination.longitude={_details.XCoordinate}&destination.name={_details.Name}");
            await Launcher.LaunchUriAsync(driveToUri);
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (e.Parameter?.GetType() != typeof (OmnivaLocation))
                return;
            _details = (OmnivaLocation) e.Parameter;
            if (_details != null)
            {
                tbxDescription.Text = _details.Name;
                tbxFullAddress.Text = _details.FullAddress;
                tbxServiceHours.Text = _details.ServiceHours;
            }
        }
    }
}