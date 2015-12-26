using Windows.Devices.Geolocation;
using App1.Interfaces;

namespace App1.Models
{
    public class BaseLocation : ILocation
    {
        public Geopoint Location => new Geopoint(new BasicGeoposition
        {
            Latitude = YCoordinate,
            Longitude = XCoordinate
        });

        public string Name { get; set; }

        public int Type { get; set; }

        public double XCoordinate { get; set; }
        public double YCoordinate { get; set; }
    }
}