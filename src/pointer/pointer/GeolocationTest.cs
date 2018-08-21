using Plugin.Geolocator;
using System;

namespace pointer
{
    public class GeolocationTest
    {
        public async void StartLocationListening()
        {
            var locator = CrossGeolocator.Current;
            locator.DesiredAccuracy = 1;
            locator.PositionChanged += (sender, e) =>
            {
                Console.WriteLine(e.Position.Longitude + ", " + e.Position.Latitude);
            };

            await locator.StartListeningAsync(new TimeSpan(1), 1, true);
        }

    }
}
