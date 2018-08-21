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
        }

        void Compass_ReadingChanged(object sender, CompassChangedEventArgs e)
        {
            var data = e.Reading;
            headingNorth = data.HeadingMagneticNorth;

            Device.BeginInvokeOnMainThread(() =>
            {
                loc.Text = $"Position: {Math.Round(longitude,3)},{Math.Round(latitude,3)} Compass: {Math.Round(headingNorth,0)} ";
            });

            //  todo: 
            // - calculate fov
            // - get objects in fov
            // - display objects

        }

    }
}
