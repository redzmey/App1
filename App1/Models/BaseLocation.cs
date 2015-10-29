using Windows.Devices.Geolocation;
using App1.Interfaces;

namespace App1
{
    public  class BaseLocation : ILocation
    {
        public string Name { get; set; }

        public double XCoordinate { get; set; }
        public double YCoordinate { get; set; }

        public Geopoint Location => new Geopoint(new BasicGeoposition
        {
            Latitude = YCoordinate,
            Longitude = XCoordinate
        });
    }
}