using System;
using Xamarin.Essentials;

namespace pointer
{
    public class CompassTest
    {

        // Set speed delay for monitoring changes.
        SensorSpeed speed = SensorSpeed.UI;

        public CompassTest()
        {
            // Register for reading changes, be sure to unsubscribe when finished
            Compass.ReadingChanged += Compass_ReadingChanged;
            // Compass.ApplyLowPassFilter = true;
        }

        void Compass_ReadingChanged(object sender, CompassChangedEventArgs e)
        {
            var data = e.Reading;
            Console.WriteLine($"Reading: {data.HeadingMagneticNorth} degrees");
            // Process Heading Magnetic North
        }

        public void ToggleCompass()
        {
            try
            {
                if (Compass.IsMonitoring)
                    Compass.Stop();
                else
                    Compass.Start(speed);
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Feature not supported on device
            }
            catch (Exception ex)
            {
                // Some other exception has occurred
            }
        }
    }
}
