using Plugin.Geolocator;
using Plugin.Permissions.Abstractions;
using System;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace pointer
{
    public partial class MainPage : ContentPage
    {
        private double headingNorth;
        private double longitude;
        private double latitude;

        public MainPage()
        {
            InitializeComponent();
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            var hasPermission = await Utils.CheckPermissions(Permission.Location);

            if (!hasPermission)
                return;

            // start compass
            Compass.Start(SensorSpeed.UI);
            Compass.ReadingChanged += Compass_ReadingChanged;

            // start position 
            var locator = CrossGeolocator.Current;
            locator.DesiredAccuracy = 1;
            locator.PositionChanged += Locator_PositionChanged;
            await locator.StartListeningAsync(new TimeSpan(1), 1, true);
        }

        private void Locator_PositionChanged(object sender, Plugin.Geolocator.Abstractions.PositionEventArgs e)
        {
            this.longitude = e.Position.Longitude;
            this.latitude = e.Position.Latitude;
            DoWork();
        }

        void Compass_ReadingChanged(object sender, CompassChangedEventArgs e)
        {
            var data = e.Reading;
            headingNorth = data.HeadingMagneticNorth;
        }

        private void DoWork()
        {
            if (longitude > 0 && latitude > 0)
            {
                var delta = 0.001;
                var env = $"{longitude - delta},{latitude - delta},{longitude + delta},{latitude + delta}";
                var features = Ngr.GetPanden(env);
                Console.WriteLine(features.Count);

                // todo: filter based on rotation device, or/and distance

                var res = "";

                foreach (var f in features)
                {
                    res += f.Properties["bouwjaar"] + ", ";
                }


                Device.BeginInvokeOnMainThread(() =>
                {
                    loc.Text = "hallo:" + res;
                });
            }
        }
    }
}
