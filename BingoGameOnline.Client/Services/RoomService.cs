using System.Net.Http.Json;
using BingoGameOnline.Client.Models;

namespace BingoGameOnline.Client.Services
{
    public class RoomService
    {
        private readonly HttpClient _http;
        public RoomService(HttpClient http)
        {
            _http = http;
        }

        public async Task<bool> CreateRoom(string name)
        {
            var response = await _http.PostAsJsonAsync("api/room/create", new { name });
            return response.IsSuccessStatusCode;
        }

        public async Task<List<RoomDto>> ListRooms()
        {
            var rooms = await _http.GetFromJsonAsync<List<RoomDto>>("api/room/list");
            return rooms ?? new List<RoomDto>();
        }

        public async Task<bool> JoinRoom(int roomId)
        {
            var response = await _http.PostAsync($"api/room/join/{roomId}", null);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> DeleteRoom(int roomId)
        {
            var response = await _http.DeleteAsync($"api/room/delete/{roomId}");
            return response.IsSuccessStatusCode;
        }
    }
}
