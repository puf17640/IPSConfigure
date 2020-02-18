using System.Collections.Generic;

namespace IPSConfigure.Models
{
    class User
    {
        public string Id { get; set; }
        public List<Room> Rooms { get; set; }
    }
}
