using IndoorKonfiguration.Services;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;

namespace IPSConfigure.Models
{
    public class Beacon
    {
        public string Id { get; set; }
        public string Uuid { get; set; }
        public string Hwid { get; set; }
        public int Major { get; set; }
        public int Minor { get; set; }
        public int Rssi { get; set; }
        public string Type { get; set; }
        public string Room { get; set; }
        public string MinorMajorString => $"{Major}.{Minor}";
        public string RoomsString => string.Join(",", DataStore.Instance.Rooms.Where(r => r.Beacons.Contains(this)).Select(r => r.Title));

        public Beacon() { }

        public Beacon(UniversalBeacon.Library.Core.Entities.Beacon original)
        {
            Original = original;
            original.PropertyChanged += (s, e) => Rssi = original.Rssi;
        }

        [JsonIgnore]
        public UniversalBeacon.Library.Core.Entities.Beacon Original;

        [JsonIgnore]
        public List<int> RssiHistory = new List<int>();

        [JsonIgnore]
        public float RssiAverage => RssiHistory.Count > 0 ? RssiHistory.Sum() / RssiHistory.Count : Rssi;
    }
}
