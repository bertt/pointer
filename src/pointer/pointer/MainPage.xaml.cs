using GeoJSON.Net.Feature;
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
        private double minimumDistance = 10;
        private double accuracy;

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
            accuracy = e.Position.Accuracy;
            longitude = e.Position.Longitude;
            latitude = e.Position.Latitude;
        }

        void Compass_ReadingChanged(object sender, CompassChangedEventArgs e)
        {
            var data = e.Reading;
            headingNorth = data.HeadingMagneticNorth;
        }

        void OnButtonClicked(object sender, EventArgs args)
        {
            DoWork();
        }

        private void DoWork()
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                tijd.Text = "tijd: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                richting.Text = "richting: " + Math.Round(headingNorth, 0) + "°";
            });

            if (longitude > 0 && latitude > 0 && Math.Round(accuracy, 0)<20)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    gps.Text = "gps: " + Math.Round(longitude,4) + ", " + Math.Round(latitude,4);
                    gpsaccuracy.Text="accuracy: " + Math.Round(accuracy,0) + "m";
                });

                var current = Connectivity.NetworkAccess;

                if (current == NetworkAccess.Internet)
                {
                    var loc1 = new NetTopologySuite.Geometries.Point(longitude, latitude);
                    // create a line
                    var line = LineCreator.GetLine(longitude, latitude, headingNorth);
                    var env = line.Envelope;

                    var env1 = $"{env.Coordinates[0].X}, {env.Coordinates[0].Y}, {env.Coordinates[2].X}, {env.Coordinates[2].Y}";

                    var features = Ngr.GetPanden(env1);

                    Feature nearestPand = null;
                    double distanceNearest;

                    (nearestPand,distanceNearest) = PandSearch.GetNearestPand(features, loc1, line);

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        afstand.Text = "afstand: " + Math.Round(distanceNearest * 111000, 0) + "m";

                        if (nearestPand != null)
                        {
                            var v = Ngr.GetVerblijfsObjecten(nearestPand.Properties["identificatie"].ToString());
                            if (v.Count > 0)
                            {
                                var adres1 = v[0].Properties["openbare_ruimte"] + " " + v[0].Properties["huisnummer"] + " " + v[0].Properties["woonplaats"];
                                adres.Text = adres1;
                            }
                            identificatie.Text = "identificatie: " + nearestPand.Properties["identificatie"];
                            bouwjaar.Text = "bouwjaar: " + nearestPand.Properties["bouwjaar"];
                            status.Text = "status: " + nearestPand.Properties["status"];
                            gebruiksdoel.Text = "gebruiksdoel: " + nearestPand.Properties["gebruiksdoel"];
                            oppervlakte.Text = "oppervlakte: " + nearestPand.Properties["oppervlakte_max"];
                            verblijfsobjecten.Text = "verblijfsobjecten: " + nearestPand.Properties["aantal_verblijfsobjecten"];
                        }
                        else
                        {
                            afstand.Text = "helaas, geen pand gevonden binnen 100m";
                            identificatie.Text = "";
                            bouwjaar.Text = "";
                            adres.Text = "";
                            status.Text = "";
                            gebruiksdoel.Text = "";
                            oppervlakte.Text = "";
                            verblijfsobjecten.Text = "";
                        }
                    });
                }
                else
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        afstand.Text = "geen internet connectie :-(";
                    });
                }
            }
            else
            {
                afstand.Text = "geen nauwkeurige (<10m) gps beschikbaar. nauwkeurigheid:" + Math.Round(accuracy,0) + "m";
            }
        }

    }
}
