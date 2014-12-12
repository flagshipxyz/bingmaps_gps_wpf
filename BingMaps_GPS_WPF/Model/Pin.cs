using Microsoft.Maps.MapControl.WPF;
using System.Device.Location;

namespace BingMaps_GPS_WPF.Model
{
    public class Pin
    {
        public Pin(GeoPosition<GeoCoordinate> geoPosition)
        {
            this.GeoPosition = geoPosition;

            this.Location = new Location(geoPosition.Location.Latitude, geoPosition.Location.Longitude, geoPosition.Location.Altitude);
        }

        public GeoPosition<GeoCoordinate> GeoPosition { get; set; }

        public Location Location { get; set; }

    }
}
