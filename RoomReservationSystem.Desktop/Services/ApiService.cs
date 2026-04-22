using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace RoomReservationSystem.Desktop.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
        private string? Token { get; set; }
        public HttpClient Client
        {
            get { return _httpClient; }
        }
        public ApiService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri("https://localhost:7266/");
        }

        public void SetToken(string token)
        {
            Token = token;
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        }
    }
}
