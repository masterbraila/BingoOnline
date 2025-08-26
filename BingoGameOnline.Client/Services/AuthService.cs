using System.Net.Http.Json;

namespace BingoGameOnline.Client.Services
{
    public class AuthService
    {
        private readonly HttpClient _http;
        public AuthService(HttpClient http)
        {
            _http = http;
        }

        public async Task<bool> Register(string username, string email, string password)
        {
            var response = await _http.PostAsJsonAsync("api/auth/register", new { username, email, password });
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> Login(string username, string password)
        {
            var response = await _http.PostAsJsonAsync("api/auth/login", new { username, password });
            return response.IsSuccessStatusCode;
        }

        public async Task Logout()
        {
            await _http.PostAsync("api/auth/logout", null);
        }
    }
}
