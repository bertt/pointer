using Plugin.Geolocator;
using Plugin.Permissions.Abstractions;
using Xamarin.Forms;

namespace pointer
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();



        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();

            var c = new CompassTest();
            c.ToggleCompass();

            var hasPermission = await Utils.CheckPermissions(Permission.Location);

            if (!hasPermission)
                return;

            if (CrossGeolocator.IsSupported)
            {
                if (CrossGeolocator.Current.IsGeolocationAvailable)
                {
                    new GeolocationTest().StartLocationListening();
                }

            }
        }
    }
}
