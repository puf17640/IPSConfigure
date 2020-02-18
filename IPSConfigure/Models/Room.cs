using IPSConfigure.Utilities;
using System.Collections.Generic;

namespace IPSConfigure.Models
{
    public class Room : PropertyChangeAware
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string User { get; set; }
        private bool _isConfigured;
        public bool IsConfigured
        {
            get => _isConfigured;
            set => Set(ref _isConfigured, value);
        }

        public List<Beacon> Beacons { get; set; }
        private bool _isUnsaved;
        public bool IsUnsaved
        {
            get => _isUnsaved;
            set => Set(ref _isUnsaved, value);
        }
        public object UnsavedState { get; set; }

        public bool ShouldSerializeBeacons() => false;
        public bool ShouldSerializeIsUnsaved() => false;
        public bool ShouldSerializeUnsavedState() => false;

        public override bool Equals(object obj) => !(obj is Room other) ? false : this.Id.Equals(other.Id);
        public override int GetHashCode() => base.GetHashCode();
    }
}
