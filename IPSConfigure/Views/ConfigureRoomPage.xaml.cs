using IPSConfigure.Models;
using IPSConfigure.ViewModels;
using Plugin.SharedTransitions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IPSConfigure.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ConfigureRoomPage : ContentPage
    {
        public ConfigureRoomPage(Room model)
        {
            InitializeComponent();
            (BindingContext as ConfigureRoomViewModel).SetRoom(model);

            if (Device.RuntimePlatform == "Android")
                NavigationPage.SetHasNavigationBar(this, false);

            SharedTransitionNavigationPage.SetTransitionDuration(this, 500);
            SharedTransitionNavigationPage.SetBackgroundAnimation(this, BackgroundAnimation.None);
        }
    }
}