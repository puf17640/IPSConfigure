using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using UniversalBeacon.Library.Core.Entities;
using UniversalBeacon.Library.Core.Interfaces;
using UniversalBeacon.Library.Core.Interop;

namespace IPSConfigure.Services
{
    public class BLE : IDisposable
    {
        public BLE(IBluetoothPacketProvider provider)
        {
            if (provider != null)
            {
                manager = new BeaconManager(provider);
                manager.BeaconAdded += (s, b) => BeaconAdded.Invoke(s, b);
                provider.AdvertisementPacketReceived += (s, e) => AdvertisementPacketReceived.Invoke(s, e);
            }
        }

        private readonly BeaconManager manager;

        public EventHandler<Beacon> BeaconAdded;
        public EventHandler<BLEAdvertisementPacketArgs> AdvertisementPacketReceived;

        public ObservableCollection<Beacon> Beacons => manager.BluetoothBeacons;

        public async Task StartAsync(int timeout = 10000)
        {
            manager?.Start();
            await Task.Delay(timeout).ContinueWith((t) => Stop());
        }

        public void Stop() => manager?.Stop();

        public void Dispose() => Stop();
    }
}
