using RoomReservatingSystem.Shared;
using RoomReservatingSystem.Shared;
using RoomReservationSystem.Desktop.Services;
using RoomReservationSystem.Desktop.Services;
using RoomReservationSystem.Desktop.UserControls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RoomReservationSystem.Desktop.UserControls
{
    public partial class RoomsControl : UserControl
    {
        private ApiService? _apiService;
        public async void Setup(ApiService apiService)
        {
            _apiService = apiService;
            await LoadRoomsAsync();
        }
        public RoomsControl()
        {
            InitializeComponent();
        }

        private async Task LoadRoomsAsync()
        {
            var rooms = await _apiService.Client.GetFromJsonAsync<List<Room>>("Api/Rooms");
            RoomsDG.ItemsSource = rooms;
        }

        private async void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = RoomsDG.SelectedItems;
            if (selected.Count == 0)
            {
                MessageBox.Show("Please select at least one record to delete.", "No Selection", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var deletionRooms = selected.Cast<Room>().ToList();
            bool needsReload = false;

            foreach (var room in deletionRooms)
            {
                try
                {
                    var response = await _apiService.Client.DeleteAsync($"Api/Rooms/{room.Id}");
                    if (response.IsSuccessStatusCode)
                    {
                        needsReload = true;
                    }
                    else
                    {
                        MessageBox.Show($"Room: {room.Id} - {room.Name} cannot be deleted. It probably has some reservations.", "Delete Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show($"Network error while trying to delete room: {room.Name}.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            if (needsReload)
            {
                await LoadRoomsAsync();
            }
        }

        private async void EditButton_Click(object sender, RoutedEventArgs e)
        {
            var selected = RoomsDG.SelectedItems;
            if (selected.Count == 0)
            {
                MessageBox.Show("Please select at least one record to edit.", "No Selection", MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                return;
            }
            var editedRooms = selected.Cast<Room>().ToList();

            Dialogs.RoomDialog editRoomDialog = new Dialogs.RoomDialog(_apiService, editedRooms);

            bool? result = editRoomDialog.ShowDialog();

            if (result == true)
            {
                await LoadRoomsAsync();
            }
        }

        private async void AddButton_Click(object sender, RoutedEventArgs e)
        {
            Dialogs.RoomDialog newRoomDialog = new Dialogs.RoomDialog(_apiService);

            bool? result = newRoomDialog.ShowDialog();

            if (result == true)
            {
                await LoadRoomsAsync();
            }
        }

        private void RoomsDG_OnSelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            if(RoomsDG.SelectedItems.Count == 0)
            {
                deleteBTN.IsEnabled = false;
                deleteBTN.IsEnabled = false;
                editBTN.Content = "Edit";
                deleteBTN.Content = "Delete";
            }
            else if(RoomsDG.SelectedItems.Count == 1)
            {
                deleteBTN.IsEnabled = true;
                editBTN.IsEnabled = true;
                var selected = RoomsDG.SelectedItems.Cast<Room>().FirstOrDefault();
                deleteBTN.Content = $"Delete {selected?.Name}";
                editBTN.Content = $"Edit {selected?.Name}";
            }
            else
            {
                deleteBTN.Content = $"Delete selected rooms";
                editBTN.Content = $"Edit selected rooms";

            }
        }
    }
}
