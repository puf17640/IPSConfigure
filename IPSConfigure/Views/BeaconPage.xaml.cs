using IPSConfigure.Models;
using IPSConfigure.ViewModels;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IPSConfigure.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class BeaconPage : ContentPage
    {
        public BeaconPage(Beacon beacon)
        {
            InitializeComponent();
            var bc = BindingContext as BeaconViewModel;
            bc.SetBeacon(beacon);

            if (Device.RuntimePlatform == "Android")
                NavigationPage.SetHasNavigationBar(this, false);
        }
    }
}