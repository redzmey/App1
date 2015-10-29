using Windows.Devices.Geolocation;

namespace App1.Interfaces
{
    public interface ILocation
    {
        string Name { get; set; }
        double XCoordinate { get; set; }
        double YCoordinate { get; set; }
        Geopoint Location { get; }
    }
}