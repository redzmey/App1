using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Navigation;
using CsvHelper;
using CsvHelper.Configuration;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace App1
{
    /// <summary>
    ///     An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            NavigationCacheMode = NavigationCacheMode.Required;
        }

        /// <summary>
        ///     Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">
        ///     Event data that describes how this page was reached.
        ///     This parameter is typically used to configure the page.
        /// </param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you

            // 
            DoMe();
        }

        public Stream GenerateStreamFromString(string s)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }


        public async void DoMe()
        {
            StreamReader sRead;
            if (NetworkInformation.GetInternetConnectionProfile().GetNetworkConnectivityLevel() >=
                NetworkConnectivityLevel.InternetAccess)
            {
                var client = new HttpClient();
                var places = await client.GetStringAsync("http://www.omniva.ee/locations.csv");
                sRead = new StreamReader(GenerateStreamFromString(places));
                //todo overwrite local file
            }
            else
            {
                var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(@"ms-appx:///OmnivaLocations.csv"));
                sRead = new StreamReader(await file.OpenStreamForReadAsync());
            }
            SetMyLocation();

            DrawLocations(sRead);
        }

        private void DrawLocations(StreamReader sRead)
        {
            var configuration = new CsvConfiguration {Delimiter = ";"};
            var models = new List<OmnivaLocation>();
            using (var reader = new CsvReader(sRead, configuration))
            {
                while (reader.Read())
                {
                    var location = new OmnivaLocation
                    {
                        Name = reader.GetField("NAME"),
                        Zip = reader.GetField("ZIP"),
                        XCoordinate = reader.GetField<double>("X_COORDINATE"),
                        YCoordinate = reader.GetField<double>("Y_COORDINATE"),
                        CountryCode = reader.GetField("A0_NAME")
                    };
                    models.Add(location);
                }
            }
            foreach (var model in models.Where(x=>x.CountryCode=="EE"))
            {
                SetPin(model.YCoordinate, model.XCoordinate, model.Name);
            }
        }

        private void SetPin(double latitude, double longitude, string name, bool myLocation = false)
        {
            //var pinLocation = new Geopoint(new BasicGeoposition() { Latitude = latitude, Longitude = longitude });
            //var youPin = CreatePin();
            //myMap.Children.Add(youPin);
            //MapControl.SetLocation(youPin, pinLocation);
            //MapControl.SetNormalizedAnchorPoint(youPin, new Point(0.0, 1.0));

            var iconPath = "ms-appx:///Assets/location-icon.png";
            if (myLocation)
                iconPath = "ms-appx:///Assets/youarehere.png";

            var mapIcon = new MapIcon
            {
                Image = RandomAccessStreamReference.CreateFromUri(new Uri(iconPath)),
                Title = name,
                Location = new Geopoint(new BasicGeoposition
                {
                    Latitude = latitude,
                    Longitude = longitude
                }),
                NormalizedAnchorPoint = new Point(0.5, 0.5)
            };
            myMap.MapElements.Add(mapIcon);
        }

        public async void SetMyLocation()
        {
            //if ((bool)IsolatedStorageSettings.ApplicationSettings["LocationConsent"] != true)
            //{
            //    // The user has opted out of Location. 
            //    StatusTextBlock.Text = "You have opted out of location. Use the app bar to turn location back on";
            //    return;
            //}

            var geolocator = new Geolocator {DesiredAccuracyInMeters = 50};

            try
            {
                // Request the current position
                var geoposition =
                    await geolocator.GetGeopositionAsync(TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(10));

                var position = new BasicGeoposition();

#if DEBUG
                position.Latitude = 59.4404751;
                position.Longitude = 24.7385376;
#else
                position.Latitude = geoposition.Coordinate.Latitude;
                position.Longitude = geoposition.Coordinate.Longitude;
            #endif
                SetPin(position.Latitude, position.Longitude, "You are here", true);
                myMap.Center = new Geopoint(position);
                myMap.ZoomLevel = 18;
            }
            catch (Exception ex)
            {
                if ((uint) ex.HResult == 0x80004004)
                {
                    // the application does not have the right capability or the location master switch is off
                    //   StatusTextBlock.Text = "location  is disabled in phone settings.";
                }
                //else
                {
                    // something else happened acquring the location
                }
            }
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // todo load buttons
        }
    }
}