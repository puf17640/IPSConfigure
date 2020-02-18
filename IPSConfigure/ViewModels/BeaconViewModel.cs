using IPSConfigure.Models;
using IPSConfigure.Utilities;
using System.Windows.Input;
using Xamarin.Essentials;
using Xamarin.Forms;
using XF.Material.Forms.UI.Dialogs;
using XF.Material.Forms.UI.Dialogs.Configurations;

namespace IPSConfigure.ViewModels
{
    class BeaconViewModel : PropertyChangeAware
    {
        public string UserId => Settings.Current.UserId;

        private Beacon _beacon;
        public Beacon Beacon
        {
            get => _beacon;
            set => Set(ref _beacon, value);
        }

        public void SetBeacon(Beacon beacon) => Beacon = beacon;

        public ICommand UserIdCommand => new Command<string>(async (s) =>
        {
            await Clipboard.SetTextAsync(s);
            await MaterialDialog.Instance.SnackbarAsync(message: "UserId copied to clipboard.",
                                                msDuration: MaterialSnackbar.DurationShort, new MaterialSnackbarConfiguration()
                                                {
                                                    CornerRadius = 15f,
                                                    Margin = new Thickness(20, 0, 20, 20)
                                                });
        });
    }
}
