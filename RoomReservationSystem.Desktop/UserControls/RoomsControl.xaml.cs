using RoomReservationSystem.Desktop.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using RoomReservatingSystem.Shared;
using RoomReservationSystem.Desktop.Services;
using RoomReservatingSystem.Shared;

namespace RoomReservationSystem.Desktop.UserControls
{
    public partial class RoomsControl : UserControl
    {
        private ApiService? _apiService;
        public async void Setup(ApiService apiService)
        {
            _apiService = apiService;
            await LoadUsersAsync();
        }
        public RoomsControl()
        {
            InitializeComponent();
        }

        private async Task LoadUsersAsync()
        {
            var rooms = await _apiService.Client.GetFromJsonAsync<List<Room>>("Api/Rooms");
            RoomsDG.ItemsSource = rooms;
        }
    }
}
