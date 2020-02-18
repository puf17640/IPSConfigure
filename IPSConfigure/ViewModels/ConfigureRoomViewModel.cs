using IndoorKonfiguration.Services;
using IPSConfigure.Models;
using IPSConfigure.Utilities;
using IPSConfigure.Views;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using UniversalBeacon.Library.Core.Entities;
using UniversalBeacon.Library.Core.Interop;
using Xamarin.Essentials;
using Xamarin.Forms;
using XF.Material.Forms.Models;
using XF.Material.Forms.UI.Dialogs;
using XF.Material.Forms.UI.Dialogs.Configurations;

namespace IPSConfigure.ViewModels
{
    public class ConfigureRoomViewModel : PropertyChangeAware
    {
        public string UserId => Settings.Current.UserId;

        public ConfigureRoomViewModel()
        {
            Models = new ObservableCollection<Models.Beacon>();
            App.BLEAdapter.AdvertisementPacketReceived += (object s, BLEAdvertisementPacketArgs e) =>
            {
                var data = e.Data;
                var bluetoothAddress = string.Join(":", BitConverter.GetBytes(data.BluetoothAddress).Reverse().Select(b => b.ToString("X2"))).Substring(6);
                if (!Room.Beacons.Any(b => b.Hwid.Equals(bluetoothAddress)))
                    return;
                Room.Beacons.Where(b => b.Hwid.Equals(bluetoothAddress)).FirstOrDefault().RssiHistory.Add(data.RawSignalStrengthInDBm);
            };
        }

        private Room _room;
        public Room Room
        {
            get => _room;
            set => Set(ref _room, value);
        }

        public bool HasConfig => Models.Count > 0;

        private ObservableCollection<Models.Beacon> _models;
        public ObservableCollection<Models.Beacon> Models
        {
            get => _models;
            set => Set(ref _models, value);
        }

        public string[] Actions => new string[] { "Remove" };

        public string[] ListActions => new string[] { "Search", "Clear", "Save" };

        public ICommand JobSelectedCommand => new Command<string>(async (s) =>
        {
            var selectedModel = this.Models.FirstOrDefault(m => m.Hwid == s);
            var x = new BeaconPage(selectedModel);
            await Application.Current.MainPage.Navigation.PushAsync(x);
        });

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

        public ICommand ListMenuCommand => new Command<MaterialMenuResult>(async (s) =>
        {
            if (s.Index < 0 || s.Index > ListActions.Length)
                return;
            switch (ListActions[s.Index])
            {
                case "Search":
                    if (Room.IsConfigured || HasConfig && (await MaterialDialog.Instance.ConfirmAsync("Are you sure you want to delete the current configuration and create a new one?", "New Configuration", "Yes", "No")).GetValueOrDefault())
                        await DeleteConfigAsync();
                    await StartConfigurationAsync();
                    break;

                case "Clear":
                    if ((await MaterialDialog.Instance.ConfirmAsync("Are you sure you want to delete the current configuration?", "Delete Configuration", "Yes", "No")).GetValueOrDefault())
                    {
                        using (var loadingDialog = await MaterialDialog.Instance.LoadingDialogAsync(message: $"Deleting configuration for '{Room.Title}'"))
                        {
                            var success = false;
                            try
                            {
                                success = await DeleteConfigAsync();
                                if (success)
                                {
                                    Models.Clear();
                                    Room.IsUnsaved = (!Room.IsConfigured && Models.Count > 0) || (Room.IsConfigured && !(Models.Count == Room.Beacons.Count && Models.All(b => Room.Beacons.Contains(b))));
                                    Room.UnsavedState = null;
                                }
                            }
                            catch
                            {
                                success = false;
                            }
                            finally
                            {
                                loadingDialog.MessageText = success ? "Done!" : "An error occurred, please try again.";
                                await Task.Delay(3000);
                            }
                        }
                    }
                    break;

                case "Save":
                    if (HasConfig)
                    {
                        using (var loadingDialog = await MaterialDialog.Instance.LoadingDialogAsync(message: $"Saving configuration for '{Room.Title}'"))
                        {
                            var success = false;
                            try
                            {
                                success = await SaveConfigAsync();
                                if (success)
                                {
                                    Models = new ObservableCollection<Models.Beacon>(Room.Beacons);
                                }
                                Room.IsUnsaved = (!Room.IsConfigured && Models.Count > 0) || (Room.IsConfigured && !(Models.Count == Room.Beacons.Count && Models.All(b => Room.Beacons.Contains(b))));
                                Room.UnsavedState = Models.Count > 0 ? Models : null;
                            }
                            catch
                            {
                                success = false;
                            }
                            finally
                            {
                                loadingDialog.MessageText = success ? "Done!" : "An error occurred, please try again.";
                                await Task.Delay(3000);
                            }
                        }
                    }
                    else
                        await MaterialDialog.Instance.AlertAsync("Please make sure to create a configuration before trying to save it.", "No Configuration found.");
                    break;
            }
        });
        public ICommand MenuCommand => new Command<MaterialMenuResult>(async (s) =>
        {
            var beacon = Models.Where(b => b.Hwid.Equals(s.Parameter)).FirstOrDefault();
            if (beacon == null || s.Index < 0 || s.Index > ListActions.Length)
                return;
            switch (Actions[s.Index])
            {
                case "Remove":
                    if ((await MaterialDialog.Instance.ConfirmAsync("Are you sure you want to remove this beacon from the configuration?", beacon.Hwid, "Yes", "No")).GetValueOrDefault())
                    {
                        var success = false;
                        try
                        {
                            success = Models.Remove(beacon);
                            Room.IsUnsaved = (!Room.IsConfigured && Models.Count > 0) || (Room.IsConfigured && !(Models.Count == Room.Beacons.Count && Models.All(b => Room.Beacons.Contains(b))));
                            Room.UnsavedState = Models;
                        }
                        catch
                        {
                            // Log Error
                            success = false;
                        }
                        finally
                        {
                            await MaterialDialog.Instance.SnackbarAsync(message: success ? "Beacon removed." : "Oops! An error occured.",
                                                    msDuration: MaterialSnackbar.DurationShort, new MaterialSnackbarConfiguration()
                                                    {
                                                        CornerRadius = 15f,
                                                        Margin = new Thickness(20, 0, 20, 20)
                                                    });
                        }
                    }
                    break;
            }
        });

        public void SetRoom(Room room)
        {
            Room = room;
            if (Room.UnsavedState is ObservableCollection<Models.Beacon> beacons)
                Models = beacons;
            else
                room.Beacons.ForEach(b => Models.Add(b));
        }

        private async Task StartConfigurationAsync()
        {
            using (var loadingDialog = await MaterialDialog.Instance.LoadingDialogAsync(message: $"Gathering data for '{Room.Title}'"))
            {
                try
                {
                    App.BLEAdapter.Beacons.Clear();
                    App.BLEAdapter.BeaconAdded += AddBeacon;
                    await App.BLEAdapter.StartAsync();
                    loadingDialog.MessageText = Models.Count > 0 ? "Done!" : "Couldnt get enough data, please try again.";
                }
                catch (NullReferenceException)
                {
                    loadingDialog.MessageText = "It seems you have bluetooth disabled, please enable bluetooth and try again.";
                }
                catch (Exception)
                {
                    loadingDialog.MessageText = "An error occurred, please try again.";
                }
                finally
                {
                    App.BLEAdapter.BeaconAdded -= AddBeacon;
                    await Task.Delay(3000);
                }
            }
        }

        private async Task<bool> SaveConfigAsync()
        {
            var toDelete = Room.Beacons.Except(Models.Where(m => m.Id != null));
            if (toDelete.Count() > 0)
                await Task.WhenAll(toDelete.Select(b => DataStore.Instance.DeleteBeaconAsync(b)));
            await Task.WhenAll(Models.Select(b => DataStore.Instance.AddBeaconAsync(b)));
            Room.IsConfigured = true;
            return await DataStore.Instance.UpdateRoomAsync(Room);
        }

        private async Task<bool> DeleteConfigAsync()
        {
            if (Room.Beacons.Count == 0 && Models.Count == 0) return true;
            await Task.WhenAll(Room.Beacons.Select(b => DataStore.Instance.DeleteBeaconAsync(b)));
            Room.IsConfigured = false;
            return await DataStore.Instance.UpdateRoomAsync(Room);
        }

        private void AddBeacon(object sender, UniversalBeacon.Library.Core.Entities.Beacon b)
        {
            if (b.BeaconType == UniversalBeacon.Library.Core.Entities.Beacon.BeaconTypeEnum.iBeacon)
            {
                var frame = b.BeaconFrames.Select(f => new ProximityBeaconFrame(f.Payload)).FirstOrDefault();
                if (frame == null) return;
                if (Models.Any(be => be.Hwid.Equals(b.BluetoothAddressAsString)))
                {
                    Models.Where(be => be.Hwid.Equals(b.BluetoothAddressAsString)).FirstOrDefault().Rssi = b.Rssi;
                    return;
                }
                var beacon = new Models.Beacon();
                try
                {
                    beacon.Uuid = frame.UuidAsString;
                    beacon.Type = b.BeaconType.ToString();
                    beacon.Major = frame.Major;
                    beacon.Minor = frame.Minor;
                    beacon.Hwid = b.BluetoothAddressAsString;
                    beacon.Rssi = b.Rssi;
                    beacon.Original = b;
                    beacon.Room = Room.Id;
                }
                catch (Exception)
                {
                    return;
                }
                Models.Add(beacon);
                Room.IsUnsaved = (!Room.IsConfigured && Models.Count > 0) || (Room.IsConfigured && !(Models.Count == Room.Beacons.Count && Models.All(be => Room.Beacons.Contains(be))));
                Room.UnsavedState = Models.Count > 0 ? Models : null;
            }
        }
    }
}