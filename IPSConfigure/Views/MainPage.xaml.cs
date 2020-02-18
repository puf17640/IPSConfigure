using Plugin.SharedTransitions;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace IPSConfigure.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();

            if (Device.RuntimePlatform == "Android")
                NavigationPage.SetHasNavigationBar(this, false);

            SharedTransitionNavigationPage.SetTransitionDuration(this, 500);
            SharedTransitionNavigationPage.SetBackgroundAnimation(this, BackgroundAnimation.None);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (App.CurrentRoom != null) App.CurrentRoom = null;
        }
    }
}