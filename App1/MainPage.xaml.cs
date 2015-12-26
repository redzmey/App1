using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;
using Windows.Foundation;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Maps;
using Windows.UI.Xaml.Navigation;
using App1.Helpers;
using App1.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Newtonsoft.Json;

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

        private void BtnGetall_OnClick(object sender, RoutedEventArgs e)
        {
            //List<OmnivaLocation> a = _models;
            //var listPage = new LocationsListPage(_models);
            //listPage.
            Frame.Navigate(typeof (Pages.LocationsListPage), _models);
        }

        private void btnOnMe_Click(object sender, RoutedEventArgs e)
        {
            SetMyLocation();
        }

        private async void btnSearh_Click(object sender, RoutedEventArgs e)
        {
            Pages.SearchDialog searchDialog = new Pages.SearchDialog();
            await searchDialog.ShowAsync();
            string seachString = searchDialog.SearchString.ToLower();
            if (!string.IsNullOrWhiteSpace(seachString))
            {
                myMap.MapElements.Clear();
                List<OmnivaLocation> filteredLocations =
                    _models.Where(x => x.Name.ToLower().Contains(seachString)).ToList();
                foreach (OmnivaLocation model in filteredLocations)
                    SetPin(model);

                if (filteredLocations.Count == 1)
                {
                    BasicGeoposition position = new BasicGeoposition
                    {
                        Latitude = filteredLocations[0].YCoordinate,
                        Longitude = filteredLocations[0].XCoordinate
                    };
                    myMap.Center = new Geopoint(position);
                    myMap.ZoomLevel = 15;
                }
            }
        }

        private void BtnUpdate_OnClick(object sender, RoutedEventArgs e)
        {
            UpdateFile();
        }

        private void btnZoomIn_Click(object sender, RoutedEventArgs e)
        {
            if (myMap.ZoomLevel < 25)
                myMap.ZoomLevel = ++myMap.ZoomLevel;
        }

        private void btnZoomOut_Click(object sender, RoutedEventArgs e)
        {
            if (myMap.ZoomLevel > 3)
                myMap.ZoomLevel = --myMap.ZoomLevel;
        }

        public async void DoMe()
        {
            StreamReader sRead;
            if (NetworkInformation.GetInternetConnectionProfile().GetNetworkConnectivityLevel() >=
                NetworkConnectivityLevel.InternetAccess)
            {
                HttpClient client = new HttpClient();
                string places = await client.GetStringAsync("http://www.omniva.ee/locations.csv");
                sRead = new StreamReader(GenerateStreamFromString(places));
            }
            else
            {
                StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(@"ms-appx:///DataFiles/OmnivaLocations.csv"));
                sRead = new StreamReader(await file.OpenStreamForReadAsync());
            }
            SetMyLocation();
            GetLocations(sRead);
            DrawLocations();
        }

        private void DrawLocations()
        {
            // Bounds visibleBounds = GetBounds();
            if (myMap.Children.Count != 0)
            {
                DependencyObject pushpin = myMap.Children.FirstOrDefault(p => p.GetType() == typeof (OmnivaLocation));

                if (pushpin != null)
                    myMap.Children.Remove(pushpin);
            }
            if (_models == null)
                return;
            int i = 0;
            foreach (OmnivaLocation model in _models.Where(x => x.CountryCode == "EE"))
                //foreach (var model in _models.Where(x => x.IsPointInside(visibleBounds)))
                SetPin(model);
        }

        public Stream GenerateStreamFromString(string s)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(s);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }

        private Bounds GetBounds()
        {
            GeoboundingBox geoBox = myMap.GetBounds();
            Bounds result = new Bounds
            {
                East = geoBox.SoutheastCorner.Longitude,
                North = geoBox.NorthwestCorner.Latitude,
                West = geoBox.NorthwestCorner.Longitude,
                South = geoBox.SoutheastCorner.Latitude
            };
            return result;
        }

        private async void GetLocations(StreamReader sRead)
        {
            CsvConfiguration configuration = new CsvConfiguration {Delimiter = ";"};
            _models = new List<OmnivaLocation>();
            using (CsvReader reader = new CsvReader(sRead, configuration))
            {
                while (reader.Read())
                {
                    OmnivaLocation location = new OmnivaLocation
                    {
                        Name = reader.GetField("NAME"),
                        Zip = reader.GetField("ZIP"),
                        XCoordinate = reader.GetField<double>("X_COORDINATE"),
                        YCoordinate = reader.GetField<double>("Y_COORDINATE"),
                        CountryCode = reader.GetField("A0_NAME"),
                        FullAddress =
                            string.Join(Environment.NewLine, reader.GetField("A1_NAME"), reader.GetField("A2_NAME"), reader.GetField("A3_NAME"), reader.GetField("A4_NAME"), reader.GetField("A5_NAME"),
                                        reader.GetField("A6_NAME"), reader.GetField("A7_NAME"), reader.GetField("A8_NAME")).Replace(Environment.NewLine + "NULL", ""),
                        Type = reader.GetField<int>("TYPE"),
                        ServiceHours = reader.GetField("SERVICE_HOURS")
                    };
                    _models.Add(location);
                }
            }
            await SaveJson();
        }

        private void myMap_ZoomLevelChanged(MapControl sender, object args)
        {
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

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // todo load buttons
        }

        private async Task<OmnivaJson> ReadJson()
        {
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            Stream textFile = await localFolder.OpenStreamForReadAsync("omnivaLocations.json");
            using (StreamReader streamReader = new StreamReader(textFile))
            {
                string reader = new JsonTextReader(streamReader).ToString();
                return JsonConvert.DeserializeObject<OmnivaJson>(reader);
            }
        }

        private async Task SaveJson()
        {
            OmnivaJson newOmnivaJson = new OmnivaJson
            {
                Updated = DateTime.Now,
                Locations = _models
            };
            string jsonContents = JsonConvert.SerializeObject(newOmnivaJson, Formatting.Indented);
            StorageFolder localFolder = ApplicationData.Current.LocalFolder;
            StorageFile textFile = await localFolder.CreateFileAsync("omnivaLocations.json",
                                                                     CreationCollisionOption.ReplaceExisting);
            using (IRandomAccessStream textStream = await textFile.OpenAsync(FileAccessMode.ReadWrite))
            {
                using (DataWriter textWriter = new DataWriter(textStream))
                {
                    textWriter.WriteString(jsonContents);
                    await textWriter.StoreAsync();
                }
            }
        }

        public async void SetMyLocation()
        {
            //if ((bool)IsolatedStorageSettings.ApplicationSettings["LocationConsent"] != true)
            //{
            //    // The user has opted out of Location. 
            //    StatusTextBlock.Text = "You have opted out of location. Use the app bar to turn location back on";
            //    return;
            //}

            Geolocator geolocator = new Geolocator {DesiredAccuracyInMeters = 50};

            try
            {
                // Request the current position
                Geoposition geoposition =
                    await geolocator.GetGeopositionAsync(TimeSpan.FromMinutes(5), TimeSpan.FromSeconds(10));

                if (geoposition != null)
                {
                    BasicGeoposition position = new BasicGeoposition
                    {
                        Latitude = geoposition.Coordinate.Latitude,
                        Longitude = geoposition.Coordinate.Longitude
                    };

#if DEBUG
                position.Latitude = 59.4404751;
                position.Longitude = 24.7385376;
#else
#endif
                    BaseLocation myLocation = new BaseLocation
                    {
                        XCoordinate = position.Longitude,
                        YCoordinate = position.Latitude,
                        Name = "You are here",
                        Type = 99
                    };
                    // SetPin(myLocation);
                    myMap.Center = new Geopoint(position);
                }
                myMap.ZoomLevel = 15;
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

        private void SetPin(BaseLocation location, bool myLocation = false)
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
                case 99:
                    iconPath = "ms-appx:///Assets/youarehere.png";
                    break;
                default:
                    iconPath = "ms-appx:///Assets/location-icon.png";
                    break;
            }

            CustomControls.LocationPin mapIcon = new CustomControls.LocationPin {ImagePath = iconPath, Details = location}; //iconPath,name
            myMap.Children.Add(mapIcon);
            MapControl.SetLocation(mapIcon, location.Location);
            MapControl.SetNormalizedAnchorPoint(mapIcon, new Point(0.5, 0.5));
        }

        private async void UpdateFile()
        {
            if (NetworkInformation.GetInternetConnectionProfile().GetNetworkConnectivityLevel() <
                NetworkConnectivityLevel.InternetAccess) return;
            HttpClient client = new HttpClient();
            string places = await client.GetStringAsync("http://www.omniva.ee/locations.csv");
            StreamReader sRead = new StreamReader(GenerateStreamFromString(places));

            StorageFile file = await StorageFile.GetFileFromApplicationUriAsync(new Uri(@"ms-appx:///DataFiles/OmnivaLocations.csv"));
            StreamWriter sWrite = new StreamWriter(await file.OpenStreamForWriteAsync());
            await sWrite.WriteAsync(sRead.ReadToEnd());
        }
    }
}