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
        private double accuracy;
        private int gps_minimum_accuracy = 20;
        private double maximum_distance_for_pand = 100;
        private double minimal_distance_for_pand = 10;

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
                tijd.Text = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                richting.Text = Math.Round(headingNorth, 0) + "°";
                gps.Text = Math.Round(longitude, 4) + ", " + Math.Round(latitude, 4);
                gpsaccuracy.Text = Math.Round(accuracy, 0) + "m";
            });

            if (longitude > 0 && latitude > 0 && Math.Round(accuracy, 0) < gps_minimum_accuracy)
            {
                var current = Connectivity.NetworkAccess;

                if (current == NetworkAccess.Internet)
                {
                    var loc1 = new NetTopologySuite.Geometries.Point(longitude, latitude);
                    // create a line
                    var line = LineCreator.GetLine(longitude, latitude, headingNorth, maximum_distance_for_pand);
                    var env = line.Envelope;

                    var env1 = $"{env.Coordinates[0].X}, {env.Coordinates[0].Y}, {env.Coordinates[2].X}, {env.Coordinates[2].Y}";

                    var features = Ngr.GetPanden(env1);

                    Feature nearestPand = null;
                    double distanceNearest;

                    (nearestPand, distanceNearest) = PandSearch.GetNearestPand(features, loc1, line, minimal_distance_for_pand);

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (nearestPand != null)
                        {
                            pandtext.Text = "pand gevonden!";
                            afstand.Text = Math.Round(distanceNearest * 111000, 0) + "m";
                            var v = Ngr.GetVerblijfsObjecten(nearestPand.Properties["identificatie"].ToString());
                            if (v.Count > 0)
                            {
                                var adres1 = v[0].Properties["openbare_ruimte"] + " " + v[0].Properties["huisnummer"] + " " + v[0].Properties["woonplaats"];
                                adres.Text = adres1;
                            }
                            identificatie.Text = nearestPand.Properties["identificatie"].ToString();
                            bouwjaar.Text = nearestPand.Properties["bouwjaar"].ToString();
                            status.Text = nearestPand.Properties["status"].ToString();
                            gebruiksdoel.Text = nearestPand.Properties["gebruiksdoel"] != null ? nearestPand.Properties["gebruiksdoel"].ToString() : "-";
                            oppervlakte.Text = nearestPand.Properties["oppervlakte_max"].ToString();
                            verblijfsobjecten.Text = nearestPand.Properties["aantal_verblijfsobjecten"].ToString();
                        }
                        else
                        {
                            pandtext.Text = $"helaas, geen pand gevonden binnen {maximum_distance_for_pand}m";
                            EmptyPandGrid();
                        }
                    });
                }
                else
                {
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        pandtext.Text = "geen internet connectie :-(";
                        EmptyPandGrid();
                    });
                }
            }
            else
            {
                pandtext.Text = $"geen nauwkeurige (<{gps_minimum_accuracy}m) gps beschikbaar";
                EmptyPandGrid();
            }
        }

        private void EmptyPandGrid()
        {
            afstand.Text = "-";
            identificatie.Text = "-";
            bouwjaar.Text = "-";
            status.Text = "-";
            gebruiksdoel.Text = "-";
            oppervlakte.Text = "-";
            verblijfsobjecten.Text = "-";
            adres.Text = "";
        }

    }
}
