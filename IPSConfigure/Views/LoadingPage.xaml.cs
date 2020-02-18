using IndoorKonfiguration.Services;
using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using System;
using System.Threading.Tasks;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XF.Material.Forms.UI.Dialogs;

namespace IPSConfigure.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoadingPage : ContentPage
    {
        public LoadingPage()
        {
            InitializeComponent();

            NavigationPage.SetHasNavigationBar(this, false);
        }

        private async void AppearingAsync(object sender, EventArgs e)
        {
            var statusLocation = await RequestPermAsync<LocationPermission>();
            var statusStorage = await RequestPermAsync<StoragePermission>();
            if (!statusLocation || !statusStorage)
                await MaterialDialog.Instance.AlertAsync("Please grant permission for " + (statusLocation ? $"{Permission.Storage}" : (statusStorage ? $"{Permission.LocationWhenInUse}" : $"{Permission.Storage} and {Permission.LocationWhenInUse}")), "Permission denied")
                    .ContinueWith(t => Environment.Exit(-1));
            else
                await DataStore.Instance.InitAsync().ContinueWith(t =>
                {
                    if (t.Result)
                        (Application.Current as App).Init();
                    else
                        Environment.Exit(-1);
                }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private async Task<bool> RequestPermAsync<T>() where T : BasePermission, new()
        {
            try
            {
                var status = await CrossPermissions.Current.CheckPermissionStatusAsync<T>();
                if (status != PermissionStatus.Granted)
                {
                    status = await CrossPermissions.Current.RequestPermissionAsync<T>();
                }
                return status == PermissionStatus.Granted ? true : false;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}