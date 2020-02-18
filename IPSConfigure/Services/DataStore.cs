using IPSConfigure;
using IPSConfigure.Models;
using IPSConfigure.Utilities;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IndoorKonfiguration.Services
{
    class DataStore
    {
        public static DataStore Instance { get; private set; } = new DataStore();

        private HttpClient Client { get; set; }

        private const string BASE_URL = "https://indoorserver.azurewebsites.net/api";

        public User User { get; private set; }

        public ObservableCollection<Room> Rooms { get; private set; }

        private DataStore()
        {
            Client = new HttpClient();
        }

        public async Task<bool> InitAsync()
        {
            try
            {
                User = await CreateOrGetUserAsync();
                SaveUserId();
                Rooms = new ObservableCollection<Room>(await GetRoomsAsync(true));
                return true;
            }
            catch
            {
                return false;
            }
        }

        public void SaveUserId()
        {
            Directory.CreateDirectory(App.UserPath);
            File.WriteAllText(Path.Combine(App.UserPath, "userId.txt"), Settings.Current.UserId);
        }

        private async Task<User> CreateOrGetUserAsync()
        {
            User user;
            using var cancellationTokenSource = new CancellationTokenSource(10000);
            try
            {
                if (Settings.Current.UserId == null)
                {
                    var url = $"{BASE_URL}/users";
                    using var response = await Client.PostAsync(url, null, cancellationTokenSource.Token);
                    var rep = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode)
                    {
                        user = JsonConvert.DeserializeObject<User>(rep);
                        Settings.Current.UserId = user.Id;
                    }
                    else
                        return null;
                }
                else
                {
                    var url = $"{BASE_URL}/users/{Settings.Current.UserId}";
                    using var response = await Client.GetAsync(url, cancellationTokenSource.Token);
                    var rep = await response.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode)
                    {
                        user = JsonConvert.DeserializeObject<User>(rep);
                        if (user == null)
                        {
                            Settings.Current.UserId = null;
                            return await CreateOrGetUserAsync();
                        }
                    }
                    else
                        return null;
                }
            }
            catch (TaskCanceledException)
            {
                return await CreateOrGetUserAsync();
            }
            return user;
        }

        public async Task<bool> AddBeaconAsync(Beacon item)
        {
            var room = await GetRoomAsync(item.Room);
            if (room.Beacons.Any((r) => r.Id.Equals(item.Id))) return false;
            if (item.RssiHistory.Count > 0) item.Rssi = item.RssiHistory.Sum() / item.RssiHistory.Count;
            using var cancellationTokenSource = new CancellationTokenSource(10000);
            using var response = await Client.PostAsync($"{BASE_URL}/beacons", new StringContent(JsonConvert.SerializeObject(item), Encoding.UTF8, "application/json"), cancellationTokenSource.Token);
            if (response.IsSuccessStatusCode)
            {
                item = JsonConvert.DeserializeObject<Beacon>(await response.Content.ReadAsStringAsync());
                room.Beacons.Add(item);
                return room.Beacons.Contains(item);
            }
            return false;
        }

        public async Task<bool> DeleteBeaconAsync(Beacon item) => await DeleteBeaconAsync(item.Id, item.Room);

        public async Task<bool> DeleteBeaconAsync(string id, string roomId)
        {
            var room = await GetRoomAsync(roomId);
            using var cancellationTokenSource = new CancellationTokenSource(10000);
            using var response = await Client.DeleteAsync($"{BASE_URL}/beacons/{id}", cancellationTokenSource.Token);
            return response.IsSuccessStatusCode ? room.Beacons.Remove(room.Beacons.Where(r => r.Id.Equals(id)).FirstOrDefault()) : false;
        }

        public async Task<bool> AddRoomAsync(Room item)
        {
            if (Rooms.Any((r) => r.Id.Equals(item.Id)) || item.Title == null || item.Title.Length < 1) return false;
            using var cancellationTokenSource = new CancellationTokenSource(10000);
            using var response = await Client.PostAsync($"{BASE_URL}/rooms", new StringContent(JsonConvert.SerializeObject(item), Encoding.UTF8, "application/json"), cancellationTokenSource.Token);
            if (response.IsSuccessStatusCode)
            {
                item = JsonConvert.DeserializeObject<Room>(await response.Content.ReadAsStringAsync());
                Rooms.Add(item);
                return Rooms.Contains(item);
            }
            return false;
        }

        public async Task<bool> DeleteRoomAsync(Room item) => await DeleteRoomAsync(item.Id);

        public async Task<bool> DeleteRoomAsync(string id)
        {
            using var cancellationTokenSource = new CancellationTokenSource(10000);
            using var response = await Client.DeleteAsync($"{BASE_URL}/rooms/{id}", cancellationTokenSource.Token);
            return response.IsSuccessStatusCode ? Rooms.Remove(Rooms.Where(r => r.Id.Equals(id)).FirstOrDefault()) : false;
        }

        public Task<Room> GetRoomAsync(string id) => Task.FromResult(Rooms.Where((r) => r.Id.Equals(id)).FirstOrDefault());

        public async Task<IEnumerable<Room>> GetRoomsAsync(bool forceRefresh = false)
        {
            if (!forceRefresh) return Rooms.AsEnumerable();
            using var cancellationTokenSource = new CancellationTokenSource(10000);
            using var response = await Client.GetAsync($"{BASE_URL}/users/{Settings.Current.UserId}", cancellationTokenSource.Token);
            var user = JsonConvert.DeserializeObject<User>(await response.Content.ReadAsStringAsync());
            return user != null && response.IsSuccessStatusCode ? user.Rooms : null;
        }

        public async Task<bool> UpdateRoomAsync(Room item)
        {
            var index = Rooms.IndexOf(item);
            if (index < 0) return false;
            using var cancellationTokenSource = new CancellationTokenSource(10000);
            using var response = await Client.PutAsync($"{BASE_URL}/rooms/{item.Id}", new StringContent(JsonConvert.SerializeObject(item), Encoding.UTF8, "application/json"), cancellationTokenSource.Token);
            var rep = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode && Rooms.Remove(Rooms[index]))
            {
                item = JsonConvert.DeserializeObject<Room>(rep);
                Rooms.Insert(index, item);
                return Rooms.Contains(item);
            }
            return false;
        }
    }
}
