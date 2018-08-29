using GeoAPI.Geometries;
using GeoJSON.Net.Feature;
using NetTopologySuite.IO;
using Newtonsoft.Json;
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

            if (longitude > 0 && latitude > 0)
            {
                Device.BeginInvokeOnMainThread(() =>
                {
                    gps.Text = "gps: " + Math.Round(longitude,4) + ", " + Math.Round(latitude,4);
                });

                var delta = 0.001;
                var env = $"{longitude - delta},{latitude - delta},{longitude + delta},{latitude + delta}";

                var current = Connectivity.NetworkAccess;

                if (current == NetworkAccess.Internet)
                {
                    var features = Ngr.GetPanden(env);

                    var loc1 = new NetTopologySuite.Geometries.Point(longitude, latitude);

                    // create a line
                    var c2 = new Coordinate(longitude + Math.Sin(headingNorth) * 0.01, latitude + Math.Cos(headingNorth) * 0.01);
                    var c1 = new Coordinate(longitude, latitude);

                    var line = new NetTopologySuite.Geometries.LineString(new Coordinate[] { c1, c2 });

                    Feature nearestPand = null;
                    double distanceNearest = Double.MaxValue;

                    foreach (var pand in features)
                    {
                        var geom = pand.Geometry;
                        var geomtype = geom.Type;
                        var geomstring = JsonConvert.SerializeObject(geom);
                        var reader = new GeoJsonReader();
                        // todo: check for multipolygon versus polygon

                        IGeometry pandGeom;
                        if (geomtype == GeoJSON.Net.GeoJSONObjectType.MultiPolygon)
                        {
                            pandGeom = reader.Read<NetTopologySuite.Geometries.MultiPolygon>(geomstring);
                        }
                        else
                        {
                            pandGeom = reader.Read<NetTopologySuite.Geometries.Polygon>(geomstring);
                        }

                        var intersects = pandGeom.Intersects(line);

                        if (intersects)
                        {
                            var dist = loc1.Distance(pandGeom);

                            if (dist < distanceNearest && dist > minimumDistance / 111000)
                            {
                                distanceNearest = dist;
                                nearestPand = pand;
                            }
                        }
                    }

                    Device.BeginInvokeOnMainThread(() =>
                    {
                        afstand.Text = "afstand: " + Math.Round(distanceNearest * 111000, 0) + "m";

                        if (nearestPand != null)
                        {
                            identificatie.Text = "identificatie: " + nearestPand.Properties["identificatie"];
                            bouwjaar.Text = "bouwjaar: " + nearestPand.Properties["bouwjaar"];
                            status.Text = "status: " + nearestPand.Properties["status"];
                            gebruiksdoel.Text = "gebruiksdoel: " + nearestPand.Properties["gebruiksdoel"];
                            oppervlakte.Text = "oppervlakte: " + nearestPand.Properties["oppervlakte_max"];
                            verblijfsobjecten.Text = "verblijfsobjecten: " + nearestPand.Properties["aantal_verblijfsobjecten"];
                        }
                        else
                        {
                            afstand.Text = "helaas, geen pand gevonden";
                            identificatie.Text = "";
                            bouwjaar.Text = "";
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
                afstand.Text = "gps niet beschikbaar :-(";
            }
        }
    }
}
