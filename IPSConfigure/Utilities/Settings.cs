using Plugin.Settings;
using Plugin.Settings.Abstractions;

namespace IPSConfigure.Utilities
{
    /// <summary>
    /// This is the Settings static class that can be used in your Core solution or in any
    /// of your client applications. All settings are laid out the same exact way with getters
    /// and setters. 
    /// </summary>
    public class Settings : PropertyChangeAware
    {
        private static Settings _current;
        public static Settings Current => _current ?? (_current = new Settings());

        private Settings() { }

        private static ISettings AppSettings => CrossSettings.Current;

        public string UserId
        {
            get => AppSettings.GetValueOrDefault(nameof(UserId), null);
            set
            {
                AppSettings.AddOrUpdateValue(nameof(UserId), value);
                OnPropertyChanged("UserId");
            }
        }

    }
}
