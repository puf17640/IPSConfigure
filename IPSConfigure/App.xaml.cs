using IPSConfigure.Models;
using IPSConfigure.Services;
using IPSConfigure.Views;
using Plugin.SharedTransitions;
using UniversalBeacon.Library.Core.Interfaces;
using Xamarin.Forms;

namespace IPSConfigure
{
    public partial class App : Application
    {
        public static Room CurrentRoom { get; set; }
        public static string UserPath { get; private set; }
        public static BLE BLEAdapter { get; set; }

        public App(IBluetoothPacketProvider provider, string userPath)
        {
            InitializeComponent();
            XF.Material.Forms.Material.Init(this, "Material.Configuration");
            XF.Material.Forms.Material.PlatformConfiguration.ChangeStatusBarColor(Color.White);
            BLEAdapter = new BLE(provider);
            UserPath = userPath;
            MainPage = new LoadingPage();
        }

        public void Init() => MainPage = new SharedTransitionNavigationPage(new MainPage());

    }
}
