﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Input;
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
        private OmnivaLocation _currentModel;
        private List<OmnivaLocation> _models;
        private Bounds _visibleArea;

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
            _visibleArea = new Bounds();
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
            }
            else
            {
                var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(@"ms-appx:///OmnivaLocations.csv"));
                sRead = new StreamReader(await file.OpenStreamForReadAsync());
            }
            SetMyLocation();

            DrawLocations(sRead);
        }

        private async void UpdateFile()
        {
            if (NetworkInformation.GetInternetConnectionProfile().GetNetworkConnectivityLevel() <
                NetworkConnectivityLevel.InternetAccess) return;
            var client = new HttpClient();
            var places = await client.GetStringAsync("http://www.omniva.ee/locations.csv");
            var sRead = new StreamReader(GenerateStreamFromString(places));

            var file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(@"ms-appx:///OmnivaLocations.csv"));
            var sWrite = new StreamWriter(await file.OpenStreamForWriteAsync());
            await sWrite.WriteAsync(sRead.ReadToEnd());
        }

        private void DrawLocations(StreamReader sRead)
        {
            var configuration = new CsvConfiguration {Delimiter = ";"};
            _models = new List<OmnivaLocation>();
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
                        CountryCode = reader.GetField("A0_NAME"),
                        Type = reader.GetField<int>("TYPE")
                    };
                    _models.Add(location);
                }
            }

            if (myMap.Children.Count != 0)
            {
                var pushpin = myMap.Children.FirstOrDefault(p => (p.GetType() == typeof (OmnivaLocation)));

                if (pushpin != null)
                    myMap.Children.Remove(pushpin);
            }

            foreach (var model in _models.Where(x => x.CountryCode == "EE"))
                SetPin(model);
        }

        private void SetPin(OmnivaLocation location, bool myLocation = false)
        {
            string iconPath;
            if (myLocation)
                iconPath = "ms-appx:///Assets/youarehere.png";

            switch (location.Type)
            {
                case 0:
                    iconPath = "ms-appx:///Assets/omniva_a_location.png";
                    break;
                case 1:
                    iconPath = "ms-appx:///Assets/omniva_p_location.png";
                    break;
                default:
                    iconPath = "ms-appx:///Assets/location-icon.png";
                    break;
            }

            var mapIcon = new LocationPin {ImagePath = iconPath, Details = location}; //iconPath,name
            myMap.Children.Add(mapIcon);
            MapControl.SetLocation(mapIcon, location.Location);
            MapControl.SetNormalizedAnchorPoint(mapIcon, new Point(0.5, 0.5));
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
                //SetPin(position.Latitude, position.Longitude, "You are here", 3,true);
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

        private void btnOnMe_Click(object sender, RoutedEventArgs e)
        {
            SetMyLocation();
        }

        private void btnZoomIn_Click(object sender, RoutedEventArgs e)
        {
            if (myMap.ZoomLevel < 20)
                myMap.ZoomLevel = ++myMap.ZoomLevel;
        }

        private void btnZoomOut_Click(object sender, RoutedEventArgs e)
        {
            if (myMap.ZoomLevel > 1)
                myMap.ZoomLevel = --myMap.ZoomLevel;
        }

        private void Pushpin_Tapped(object sender, TappedRoutedEventArgs e)
        {
            ShowPushpinContent((OmnivaLocation) sender);
        }

        private void ShowPushpinContent(OmnivaLocation model)
        {
            PushpinPopup.IsOpen = false;
            PushpinPopup.DataContext = model;
            PushpinPopup.IsOpen = true;
            myMap.Center = model.Location;
            // _currentModel = model;
        }

        private async void btnSearh_Click(object sender, RoutedEventArgs e)
        {
            var searchDialog = new SearchDialog();
            await searchDialog.ShowAsync();
            var seachString = searchDialog.searchString.ToLower();
            if (!string.IsNullOrWhiteSpace(seachString))
            {
                myMap.MapElements.Clear();
                var filteredLocations =
                    _models.Where(x => x.Name.ToLower().Contains(seachString)).ToList();
                foreach (var model in filteredLocations)
                    SetPin(model);

                if (filteredLocations.Any())
                {
                    var position = new BasicGeoposition
                    {
                        Latitude = filteredLocations[0].YCoordinate,
                        Longitude = filteredLocations[0].XCoordinate
                    };
                    myMap.Center = new Geopoint(position);
                    myMap.ZoomLevel = 18;
                }
            }
        }

        private void myMap_ZoomLevelChanged(MapControl sender, object args)
        {
            //todo count models in bounds
            //   if (_models == null) return;

            //   if (MapBoundsArea != null)
            //     {
            //int count = _models.Count(location => location.IsPointInside(_visibleArea));
            //btnCount.Label = count.ToString();
            //   }
        }


        public class Bounds
        {
            public double East { get; set; }
            public double West { get; set; }
            public double North { get; set; }
            public double South { get; set; }
        }

        private void btnCount_Click(object sender, RoutedEventArgs e)
        {

        }

        private void BtnUpdate_OnClick(object sender, RoutedEventArgs e)
        {
           UpdateFile();
        }
    }
}