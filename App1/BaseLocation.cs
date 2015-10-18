using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Devices.Geolocation;

namespace App1
{
    public class BaseLocation
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

